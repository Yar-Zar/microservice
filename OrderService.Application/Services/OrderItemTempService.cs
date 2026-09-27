namespace OrderService.Application.Services
{
    public class OrderItemTempService : IOrderItemTempService
    {
        private readonly IRedisHashProvider _cache;
        private readonly ILogger<OrderItemTempService> _logger;
        private readonly OrderItemDTOValidator _createValidator;
        private readonly UpdateOrderItemValidator _updateValidator;

        public OrderItemTempService(
            IRedisHashProvider cache,
            OrderItemDTOValidator createValidator,
            UpdateOrderItemValidator updateValidator,
            ILogger<OrderItemTempService> logger)
        {
            _cache = cache;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }
        public async Task<List<OrderItemDTO>> GetOrderItemTemp(string orderId)
        {
            var items = await _cache.GetAllAsync<OrderItemDTO>(orderId);
            return items;
        }
        public async Task<(ResponseModel, List<OrderItemDTO>)> AddOrderItemTemp(OrderItemDTO request, CancellationToken ct)
        {
            try
            {
                var validationResult = await _createValidator.ValidateAsync(request, ct);
                if (!validationResult.IsValid)
                {
                    throw new FluentValidation.ValidationException(validationResult.Errors);
                }
                // Check if the item already exists in the temporary cache
                var exists = await _cache.CheckExistAsync<OrderItemDTO>(request.OrderId, x => x.ProductId == request.ProductId);

                if (exists)
                {
                    throw new DuplicateException(
                        $"The product '{request.ProductId}' already exists in the temporary order draft for Order/Session ID '{request.OrderId}'."
                    );
                }

                // Save to Redis cache with an expiration time
                var isResult = await _cache.SaveAsync(
                    request.OrderId,
                    request.ProductId.ToString(),
                    request,
                    TimeSpan.FromMinutes(15)
                );

                var items = await _cache.GetAllAsync<OrderItemDTO>(request.OrderId);

                _logger.LogInformation("Temporary order item added successfully. OrderId: {OrderId}, ProductId: {ProductId}", request.OrderId, request.ProductId);

                var response = new ResponseModel
                {
                    IsSuccess = isResult,
                    Message = isResult
                        ? "Temporary order item has been added successfully."
                        : "Failed to add the item to the temporary order. Please try again.",
                    StatusCode = isResult ? "200" : "400"
                };

                return (response, items?.ToList() ?? new List<OrderItemDTO>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add temporary order item for OrderId: {OrderId}, ProductId: {ProductId}", request.OrderId, request.ProductId);
                throw;
            }
        }

        public async Task<(ResponseModel, List<OrderItemDTO>)> UpdateOrderItemTemp(OrderItemDTO request, CancellationToken ct)
        {
            var validationResult = await _updateValidator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                throw new FluentValidation.ValidationException(validationResult.Errors);
            }

            // Check if the item exists in the temporary cache before updating
            var record = await _cache.GetAsync<OrderItemDTO>(request.OrderId, request.ProductId.ToString());

            if (record == null)
            {
                throw new NotFoundException(
                    $"The product '{request.ProductId}' was not found in the temporary order draft for Order/Session ID '{request.OrderId}'."
                );
            }

            // Perform the update in Redis cache
            var isResult = await _cache.UpdateAsync(
                request.OrderId,
                request.ProductId.ToString(),
                request,
                TimeSpan.FromMinutes(15)
            );
            var items = await _cache.GetAllAsync<OrderItemDTO>(request.OrderId);

            _logger.LogInformation("Temporary order item updated successfully. OrderId: {OrderId}, Id: {Id}", request.OrderId, request.Id);

            var response = new ResponseModel
            {
                IsSuccess = isResult,
                Message = isResult
                    ? "Temporary order item has been updated successfully."
                    : "Failed to update the temporary order item. Please try again.",
                StatusCode = isResult ? "200" : "400"
            };

            return (response, items?.ToList() ?? new List<OrderItemDTO>());


        }

        public async Task<(ResponseModel, List<OrderItemDTO>)> DeleteOrderItemTemp(string orderId, string productId)
        {
            // If a specific Product ID is provided, verify its existence before deletion
            if (!string.IsNullOrEmpty(productId))
            {
                var record = await _cache.GetAsync<OrderItemDTO>(orderId, productId);
                if (record == null)
                {
                    throw new NotFoundException(
                        $"The product '{productId}' was not found in the temporary order draft for Order/Session ID '{orderId}'."
                    );
                }
            }

            // Delete the field (or fields) from Redis cache
            var isResult = await _cache.DeleteFieldAsync(orderId, productId);
            var items = await _cache.GetAllAsync<OrderItemDTO>(orderId);

            _logger.LogInformation("Temporary order item deleted successfully. OrderId: {OrderId}, ProductId: {Id}", orderId, productId);

            var response = new ResponseModel
            {
                IsSuccess = isResult,
                Message = isResult
                    ? "Temporary order item(s) have been deleted successfully."
                    : "Failed to delete the temporary order item(s). Please try again.",
                StatusCode = isResult ? "200" : "400"
            };

            return (response, items?.ToList() ?? new List<OrderItemDTO>());

        }
    }
}
