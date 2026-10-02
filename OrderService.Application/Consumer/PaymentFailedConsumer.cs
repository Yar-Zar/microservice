using Shared.Contracts.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Consumer;

public class PaymentFailedConsumer : IConsumer<PaymentFailEvent>
{
    private readonly IOrderService _orderService;

    public PaymentFailedConsumer(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task Consume(ConsumeContext<PaymentFailEvent> context)
    {
        var orderId = context.Message.OrderId;

        await _orderService.DeleteAsync(orderId, context.CancellationToken);
    }
}
