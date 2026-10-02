using Shared.Contracts.Events;

namespace OrderService.Application.Consummer;

public class PaymentCompletedConsumer : IConsumer<PaymentCompletedEvent>
{
    private readonly IOrderService _orderService;

    public PaymentCompletedConsumer(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var orderId = context.Message.OrderId;

        await _orderService.UpdateOrderStatusAsync(orderId, context.CancellationToken);
    }
}
