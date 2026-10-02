using OrderService.Application.DTOs;
using OrderService.Application.Validations.Orders;
using Shared.Contracts.Events;
using Shared.GrpcContracts.Payment;
using Shared.GrpcContracts.Product;
using System.ComponentModel.DataAnnotations;
using static Shared.GrpcContracts.Payment.PaymentGrpcService;
using static Shared.GrpcContracts.Product.ProductGrpcService;

namespace OrderService.Application.Services
{
    public class Order_Service : IOrderService
    {
        private readonly IRedisHashProvider _cache;
        private readonly IOrderRepository _repo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Order_Service> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly OrderDTOValidator _validator;
        private readonly UpdateOrderValidator _updatevalidator;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly PaymentGrpcServiceClient _paymentClient;
        private readonly ProductGrpcServiceClient _productClient;
        public Order_Service(IRedisHashProvider cache, IOrderRepository repo, IHttpContextAccessor httpContextAccessor, OrderDTOValidator validator, UpdateOrderValidator updatevalidator, ILogger<Order_Service> logger, IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint, PaymentGrpcServiceClient paymentClient, ProductGrpcServiceClient productClient)
        {
            _cache = cache;
            _repo = repo;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _validator = validator;
            _updatevalidator = updatevalidator;
            _httpContextAccessor = httpContextAccessor;
            _publishEndpoint = publishEndpoint;
            _paymentClient = paymentClient;
            _productClient = productClient;
        }
        #region Order Item
        public async Task<IEnumerable<OrderItemDTO>> GetItemsByOrderIdAsync(string orderId, CancellationToken ct)
        {
            return await _repo.GetItemsByOrderIdAsync<OrderItemDTO>(orderId, ct);
        }
        #endregion

        #region Order
        public async Task<ResponseModel> SaveOrderAsync(OrderDTO dto, CancellationToken ct)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            try
            {
                var validationResult = await _validator.ValidateAsync(dto, ct);
                if (!validationResult.IsValid)
                {

                    throw new FluentValidation.ValidationException(validationResult.Errors);
                }

                var existingOrder = await _repo.GetByIdAsync(dto.Id, ct);
                if (existingOrder != null)
                {
                    throw new DuplicateException($"Order with OrderId: '{dto.Id}' already exists.");
                }

                //var cachedItems = await _cache.GetAllAsync<OrderItemDTO>(dto.Id);
                //if (cachedItems.Count == 0)
                //{
                //    throw new NotFoundException($"No order items found for OrderId: '{dto.Id}'");
                //}

                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    //dto.OrderItems = cachedItems;
                    var entity = dto.Adapt<Order>();
                    await _repo.AddAsync(entity);
                }, ct);

                // Pro-level log message with structured parameters
                _logger.LogInformation("Order saved successfully. OrderId: {OrderId}", dto.Id);

                return new ResponseModel
                {
                    IsSuccess = true,
                    Message = "Order saved successfully.",
                    StatusCode = "200"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save Order for OrderId: {OrderId}", dto.Id);
                throw;
            }
        }

        public async Task<ResponseModel> DeleteAsync(string orderId, CancellationToken ct)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            try
            {
                var existEntity = await _repo.GetByIdAsync(orderId, ct);
                if (existEntity == null)
                {
                    throw new NotFoundException($"Order was not found for OrderId: '{orderId}'.");
                }

                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    await _repo.DeleteAsync(existEntity);
                    await _repo.DeleteItemsByOrderIdAsync(orderId, ct);
                }, ct);

                _logger.LogInformation("Order deleted successfully. OrderId: {OrderId}", orderId);

                return new ResponseModel
                {
                    IsSuccess = true,
                    Message = "Order deleted successfully.",
                    StatusCode = "200"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting stock request with ID: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<ResponseModel> UpdateOrderAsync(string orderId, OrderDTO dto, CancellationToken ct)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            try
            {
                if (orderId != dto.Id)
                {
                    throw new InvalidOperationException($"OrderId in the URL '{orderId}' does not match OrderId in the body '{dto.Id}'.");
                }

                var validationResult = await _updatevalidator.ValidateAsync(dto, ct);
                if (!validationResult.IsValid)
                {

                    throw new FluentValidation.ValidationException(validationResult.Errors);
                }
                var existEntity = await _repo.GetByIdAsync(orderId, ct);
                if (existEntity == null)
                {
                    throw new NotFoundException($"Order was not found for ID: '{orderId}'.");
                }

                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    dto.Adapt(existEntity);
                    // TODO: Call _repo.UpdateAsync(existEntity) if required by your generic repository pattern
                }, ct);

                _logger.LogInformation("Order updated successfully. OrderId: {OrderId}", orderId);

                return new ResponseModel
                {
                    IsSuccess = true, // Fixed duplicate assignment typo
                    Message = "Order updated successfully.",
                    StatusCode = "200"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Order for OrderId: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetAllOrderAsync(AppFilter filter, CancellationToken ct)
        {
            try
            {
                var list = await _repo.GetAllAsync<Order>(filter, ct);

                _logger.LogInformation("Successfully retrieved {Count} Orders.", list.Count());
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all Orders.");
                throw;
            }
        }

        public async Task<OrderDTO> GetByIdAsync(string orderId, CancellationToken ct)
        {
            try
            {
                var entity = await _repo.GetByIdAsync(orderId, ct);
                if (entity == null)
                {
                    throw new NotFoundException($"Order was not found for OrderId: '{orderId}'.");
                }

                var dto = entity.Adapt<OrderDTO>();
                var items = await _repo.GetItemsByOrderIdAsync<OrderItemDTO>(orderId, ct);
                dto.OrderItems = items.ToList();

                _logger.LogInformation("Successfully retrieved Order for OrderId: {OrderId}", orderId);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Order by OrderId: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(string orderId, CancellationToken ct)
        {
            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var existEntity = await _repo.GetByIdAsync(orderId, ct);
                    if (existEntity == null)
                    {
                        
                        throw new NotFoundException($"Order was not found for ID: '{orderId}'.");
                      
                    }
                    existEntity.Status = "Completed"; // Example status update
                }, ct);

                _logger.LogInformation("Order Status updated successfully. OrderId: {OrderId}", orderId);
                await _publishEndpoint.Publish(new OrderStatusSuccessedEvent(orderId), ct);
                return true;
            }
            catch (Exception ex)
            {
                await _publishEndpoint.Publish(new OrderStatusFailedEvent(orderId), ct);
                _logger.LogError(ex, "Failed to update Order Status for OrderId: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<IEnumerable<OrderListViewDTO>> GetOrdersAsync(AppFilter filter, CancellationToken ct)
        {
           var orders = await _repo.GetAllAsync<Order>(filter, ct);
            var orderIds = orders.Select(o => o.Id).ToList();
            var productIds = orders.SelectMany(o => o.OrderItems.Select(oi => oi.ProductId)).Distinct().ToList();
            var paymentrequest = new RequestOrderIdList();
            paymentrequest.OrderIds.AddRange(orderIds);
            var paymentTask =  _paymentClient.GetPaymentsByOrderIdsAsync(paymentrequest, cancellationToken: ct);

            var productrequest = new RequestProductIds();
            productrequest.ProductIds.AddRange(productIds);
            var productTask =  _productClient.GetProductsByIdsAsync(productrequest, cancellationToken: ct);

            await Task.WhenAll(paymentTask.ResponseAsync, productTask.ResponseAsync);

            var payments = (await paymentTask).Items;
            var products = (await productTask).Items;

            //Dictionary Lookup (O(1))
            var paymentDict = payments.ToDictionary(p => p.OrderId, p => p);
            var productDict = products.ToDictionary(pr => pr.Id, pr => pr);
            var result = orders.Select(order =>
            {
                var hasPayment = paymentDict.TryGetValue(order.Id, out var payment);

                var orderItemDtos = order.OrderItems.Select(item =>
                {
                    var hasProduct = productDict.TryGetValue(item.ProductId, out var product);

                    return new OrderItemDetailDTO
                    {
                        ProductId = item.ProductId,
                        ProductName = hasProduct ? product.Name : "-",
                        Quantity = item.Quantity,
                        UnitPrice = hasProduct ? (decimal)product.Price : 0,
                        TotalPrice = hasProduct ? (decimal)product.Price * item.Quantity : 0
                    };
                }).ToList();

                return new OrderListViewDTO
                {
                    OrderId = order.Id,
                    OrderStatus = order.Status,
                    PaymentId = payment?.Id,
                    Amount = payment != null ? (decimal)payment.Amount : 0,
                    Currency = payment?.Currency,
                    PaymentStatus = payment?.PaymentStatus,
                    Items = orderItemDtos
                };
            }).ToList();
             return result;
        }
        #endregion
    }
}
