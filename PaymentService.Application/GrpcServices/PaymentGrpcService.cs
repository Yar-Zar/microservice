using Grpc.Core;
using Shared.GrpcContracts.Payment;
using static Shared.GrpcContracts.Payment.PaymentGrpcService;

namespace PaymentService.Application.GrpcServices;

public class PaymentGrpcService : PaymentGrpcServiceBase
{
    private readonly IPaymentRepository _paymentRepo;

    public PaymentGrpcService(IPaymentRepository paymentRepo)
    {
        _paymentRepo = paymentRepo;
    }

    public override async Task<RefundPaymentResponse> RefundPayment(
        RefundPaymentRequest request, ServerCallContext context)
    {   
        var entity = await _paymentRepo.GetByOrderIdAsync(request.OrderId, context.CancellationToken);
        await _paymentRepo.DeleteAsync(entity);
        return new RefundPaymentResponse { Success = true, Message = "Refund processed" };
    }
    public override async Task<PaymentListResponse> GetPaymentsByOrderIds(
        RequestOrderIdList request,
        ServerCallContext context)
    {
        
        var payments = await _paymentRepo.GetByOrderIdsAsync(request.OrderIds, context.CancellationToken);

       
        var response = new PaymentListResponse();
        response.Items.AddRange(payments);
        return response;
    }

}
