using Shared.Contracts.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Consumers;

public class OrderStatusSuccessedConsumer : IConsumer<OrderStatusSuccessedEvent>
{
    private readonly IPaymentService _service;

    public OrderStatusSuccessedConsumer(IPaymentService service)
    {
        _service = service;
    }

    public async Task Consume(ConsumeContext<OrderStatusSuccessedEvent> context)
    {
        var orderId = context.Message.OrderId;

        await _service.UpdatePaymentStatusAsync(orderId,"Paid", context.CancellationToken);
    }
}
