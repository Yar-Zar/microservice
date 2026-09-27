namespace OrderService.Application.Interfaces.IService;

public interface IOrderItemTempService
{
    public Task<List<OrderItemDTO>> GetOrderItemTemp(string orderId);
    public Task<(ResponseModel, List<OrderItemDTO>)> AddOrderItemTemp(OrderItemDTO request, CancellationToken ct);
    public Task<(ResponseModel, List<OrderItemDTO>)> UpdateOrderItemTemp(OrderItemDTO request, CancellationToken ct);
    public Task<(ResponseModel, List<OrderItemDTO>)> DeleteOrderItemTemp(string orderId, string productId);
}
