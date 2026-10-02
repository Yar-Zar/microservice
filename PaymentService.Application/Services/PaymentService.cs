using Shared.Contracts.Events;
using Shared.GrpcContracts.Order;

namespace PaymentService.Application.Services
{
    public class Payment_Service : IPaymentService
    {

        private readonly IPaymentRepository _repo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Payment_Service> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PaymentDTOValidator _validator;
        private readonly UpdatePaymentDTOValidator _updatevalidator;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly OrderGrpcService.OrderGrpcServiceClient _orderClient;
        public Payment_Service(IPaymentRepository repo, IPublishEndpoint publishEndpoint, OrderGrpcService.OrderGrpcServiceClient orderClient ,IHttpContextAccessor httpContextAccessor, PaymentDTOValidator validator, UpdatePaymentDTOValidator updatevalidator, ILogger<Payment_Service> logger, IUnitOfWork unitOfWork)
        {

            _repo = repo;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _validator = validator;
            _updatevalidator = updatevalidator;
            _publishEndpoint = publishEndpoint;
            _httpContextAccessor = httpContextAccessor;
            _orderClient = orderClient;
        }

        #region Payment
        public async Task<ResponseModel> SavePaymentAsync(PaymentDTO dto, CancellationToken ct)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            try
            {
                var validationResult = await _validator.ValidateAsync(dto, ct);
                if (!validationResult.IsValid)
                {

                    throw new FluentValidation.ValidationException(validationResult.Errors);
                }

                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {

                    var entity = dto.Adapt<Payment>();
                    await _repo.AddAsync(entity);
                }, ct);


                _logger.LogInformation("Payment saved successfully. PaymentId: {PaymentId}", dto.Id);
                if (dto.PaymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase))
                {
                   // await _publishEndpoint.Publish<CreatePaymentEvent>(new(dto.OrderId), ct);// asynchronously publish the event to the message broker
                    var response = await _orderClient.UpdateOrderStatusAsync(new UpdateOrderStatusRequest
                    {
                        OrderId = dto.OrderId,
                        Status = "Completed"
                    }, cancellationToken: ct);// Call the gRPC service to update the order status
                    if (!response.Success)
                    {
                        await DeleteAsync(dto.Id, ct); // Rollback the payment if order status update fails
                    }
                }
                return new ResponseModel
                {
                    IsSuccess = true,
                    Message = "Payment saved successfully.",
                    StatusCode = "200"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save Payment for PaymentId: {PaymentId}", dto.Id);
                throw;
            }

        }

        public async Task<ResponseModel> DeleteAsync(int id, CancellationToken ct)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            try
            {
                var existEntity = await _repo.GetByIdAsync(id, ct);
                if (existEntity == null)
                {
                    throw new NotFoundException($"Payment was not found for PaymentId: '{id}'.");
                }

                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    await _repo.DeleteAsync(existEntity);
                }, ct);

                _logger.LogInformation("Payment deleted successfully. PaymentId: {PaymentId}", id);

                return new ResponseModel
                {
                    IsSuccess = true,
                    Message = "Payment deleted successfully.",
                    StatusCode = "200"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting stock request with ID: {PaymentId}", id);
                throw;
            }
        }

        public async Task<ResponseModel> UpdatePaymentAsync(int PaymentId, PaymentDTO dto, CancellationToken ct)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            try
            {
                if (PaymentId != dto.Id)
                {
                    throw new InvalidOperationException($"PaymentId in the URL '{PaymentId}' does not match PaymentId in the body '{dto.Id}'.");
                }

                var validationResult = await _updatevalidator.ValidateAsync(dto, ct);
                if (!validationResult.IsValid)
                {

                    throw new FluentValidation.ValidationException(validationResult.Errors);
                }
                var existEntity = await _repo.GetByIdAsync(PaymentId, ct);
                if (existEntity == null)
                {
                    throw new NotFoundException($"Payment was not found for ID: '{PaymentId}'.");
                }

                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    dto.Adapt(existEntity);

                }, ct);

                _logger.LogInformation("Payment updated successfully. PaymentId: {PaymentId}", PaymentId);

                return new ResponseModel
                {
                    IsSuccess = true, // Fixed duplicate assignment typo
                    Message = "Payment updated successfully.",
                    StatusCode = "200"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Payment for PaymentId: {PaymentId}", PaymentId);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentDTO>> GetAllPaymentAsync(AppFilter filter, CancellationToken ct)
        {
            return await _repo.GetAllAsync<PaymentDTO>(filter, ct);
        }

        public async Task<PaymentDTO> GetByIdAsync(int PaymentId, CancellationToken ct)
        {
            try
            {
                var entity = await _repo.GetByIdAsync(PaymentId, ct);
                if (entity == null)
                {
                    throw new NotFoundException($"Payment was not found for PaymentId: '{PaymentId}'.");
                }

                var dto = entity.Adapt<PaymentDTO>();


                _logger.LogInformation("Successfully retrieved Payment for PaymentId: {PaymentId}", PaymentId);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Payment by PaymentId: {PaymentId}", PaymentId);
                throw;
            }
        }

        public async Task<PaymentDTO> GetByOrderIdAsync(string orderId, CancellationToken ct)
        {
            try
            {
                var entity = await _repo.GetByOrderIdAsync(orderId, ct);
                if (entity == null)
                {
                    throw new NotFoundException($"Payment was not found with OrderId: '{orderId}'.");
                }

                var dto = entity.Adapt<PaymentDTO>();


                _logger.LogInformation("Successfully retrieved Payment with OrderId: {OrderId}", orderId);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Payment by OrderId: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> UpdatePaymentStatusAsync(string orderId, string status, CancellationToken ct)
        {
            var existEntity = await _repo.GetByOrderIdAsync(orderId, ct);
            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                   
                    if (existEntity == null)
                    {
                        throw new NotFoundException($"Payment was not found for OrderId: '{orderId}'.");
                    }
                    existEntity.PaymentStatus = status;
                }, ct);

                _logger.LogInformation("Payment Status updated successfully. OrderId: {OrderId}", orderId);
                return true;
            }
            catch (Exception ex)
            {
             
                await _publishEndpoint.Publish(new PaymentFailEvent(orderId), ct);
                _logger.LogError(ex, "Failed to update Payment Status for OrderId: {OrderId}", orderId);
                throw;
            }
        }
        #endregion
    }
}
