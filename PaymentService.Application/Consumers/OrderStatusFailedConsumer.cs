using Shared.Contracts.Events;

namespace PaymentService.Application.Consumers;

public class OrderStatusFailedConsumer : IConsumer<OrderStatusFailedEvent>
{
    private readonly IPaymentService _service;

    public OrderStatusFailedConsumer(IPaymentService service)
    {
        _service = service;
    }

    public async Task Consume(ConsumeContext<OrderStatusFailedEvent> context)
    {
        var orderId = context.Message.OrderId;

        await _service.UpdatePaymentStatusAsync(orderId,"Fail", context.CancellationToken);
    }
}
