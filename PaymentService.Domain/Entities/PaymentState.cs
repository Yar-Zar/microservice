using MassTransit;
namespace PaymentService.Domain.Entities;
public class PaymentState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
    public string OrderId { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
