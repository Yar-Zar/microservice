namespace OrderService.Application.Interfaces.IRepository;
public interface IOrderRepository
{
    #region Order
    public Task AddAsync(Order entity);
    public Task DeleteAsync(Order entity);
    public Task<Order?> GetAsync(Expression<Func<Order, bool>> predicate, CancellationToken ct);
    public Task<bool> CheckExistsAsync(Expression<Func<Order, bool>> predicate);
    public Task<Order> GetByIdAsync(string id, CancellationToken ct);
    public Task<IEnumerable<T>> GetAllAsync<T>(AppFilter filter, CancellationToken ct);
    #endregion

    #region OrderItem
    public Task<OrderItem> GetAsync(Expression<Func<OrderItem, bool>> predicate, CancellationToken ct);
    public Task<IEnumerable<T>> GetItemsByOrderIdAsync<T>(string orderId, CancellationToken ct = default);
    public Task DeleteItemsByOrderIdAsync(string id, CancellationToken ct);
    #endregion
}

