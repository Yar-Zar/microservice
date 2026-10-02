using Grpc.Core;
using Shared.GrpcContracts.Order;
using static Shared.GrpcContracts.Order.OrderGrpcService;
namespace OrderService.Application.GrpcService;

public class OrderGrpcService : OrderGrpcServiceBase
{
    private readonly IOrderService _orderAppService;

    public OrderGrpcService(IOrderService orderAppService)
    {
        _orderAppService = orderAppService;
    }

    public override async Task<UpdateOrderStatusResponse> UpdateOrderStatus(
        UpdateOrderStatusRequest request, ServerCallContext context)
    {
       var result = await _orderAppService.UpdateOrderStatusAsync(request.OrderId, context.CancellationToken);
        return new UpdateOrderStatusResponse { Success = result, Message = result ? "Updated successfully":"Failed!" };
    }
}
