using Shared.GrpcContracts.Payment;

namespace PaymentService.Application.Interfaces.IRepository;
public interface IPaymentRepository
{
    #region Order
    public Task AddAsync(Payment entity);
    public Task DeleteAsync(Payment entity);
    public Task<Payment?> GetAsync(Expression<Func<Payment, bool>> predicate, CancellationToken ct);
    public Task<Payment> GetByIdAsync(int id, CancellationToken ct);
    public Task<Payment> GetByOrderIdAsync(string orderId, CancellationToken ct);
    public Task<IEnumerable<T>> GetAllAsync<T>(AppFilter filter, CancellationToken ct);
    public Task<List<PaymentResponseDto>> GetByOrderIdsAsync(IEnumerable<string> orderIds, CancellationToken ct);
    #endregion
}

