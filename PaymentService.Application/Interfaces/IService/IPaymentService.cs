namespace PaymentService.Application.Interfaces.IService;
public interface IPaymentService
{
    public Task<ResponseModel> SavePaymentAsync(PaymentDTO dto, CancellationToken ct);
    public Task<ResponseModel> DeleteAsync(int paymentId, CancellationToken ct);
    public Task<ResponseModel> UpdatePaymentAsync(int paymentId, PaymentDTO dto, CancellationToken ct);
    public Task<IEnumerable<PaymentDTO>> GetAllPaymentAsync(AppFilter filter, CancellationToken ct);
    public Task<PaymentDTO> GetByIdAsync(int paymentId, CancellationToken ct);
    public Task<PaymentDTO> GetByOrderIdAsync(string orderId, CancellationToken ct);
}
