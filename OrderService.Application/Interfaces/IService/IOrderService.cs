namespace OrderService.Application.Interfaces.IService;

public interface IOrderService
{
  
    public Task<IEnumerable<OrderItemDTO>> GetItemsByOrderIdAsync(string orderId, CancellationToken ct);
    public Task<ResponseModel> SaveOrderAsync(OrderDTO dto, CancellationToken ct);
    public Task<ResponseModel> DeleteAsync(string orderId, CancellationToken ct);
    public Task<ResponseModel> UpdateOrderAsync(string orderId, OrderDTO dto, CancellationToken ct);
    public Task<IEnumerable<Order>> GetAllOrderAsync(AppFilter filter, CancellationToken ct);
    public Task<OrderDTO> GetByIdAsync(string orderId, CancellationToken ct);
}
