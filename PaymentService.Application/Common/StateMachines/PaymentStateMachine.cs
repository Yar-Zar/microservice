using Shared.Contracts.Events;

namespace PaymentService.Application.Common.StateMachines
{
    public class PaymentStateMachine : MassTransitStateMachine<PaymentState>
    {
        public State PaymentPending { get; private set; } = null!;
        public State OrderUpdating { get; private set; } = null!;
        public State Completed { get; private set; } = null!;
        public State Faulted { get; private set; } = null!;
        public Event<CreatePaymentEvent> CreatePayment { get; private set; } = null!;
        public Event<OrderStatusSuccessedEvent> OrderStatusSuccessed { get; private set; } = null!;
        public Event<OrderStatusFailedEvent> OrderStatusFailed { get; private set; } = null!;
        public PaymentStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => CreatePayment, x =>
             x.CorrelateBy(s => s.OrderId, m => m.Message.OrderId)
              .SelectId(context => Guid.NewGuid()));
            Event(() => OrderStatusSuccessed, x => x.CorrelateBy(s => s.OrderId, m => m.Message.OrderId));
            Event(() => OrderStatusFailed, x => x.CorrelateBy(s => s.OrderId, m => m.Message.OrderId));

            Initially(
                When(CreatePayment)
                    .Then(context =>
                    {

                        context.Saga.OrderId = context.Message.OrderId;
                        context.Saga.CreatedAt = DateTime.UtcNow;
                    })
                    .TransitionTo(OrderUpdating)
                    .Publish(context => new PaymentCompletedEvent(context.Message.OrderId))
            );

            During(OrderUpdating,
                When(OrderStatusSuccessed)
                    .Then(context =>
                    {
                        
                    })
                    .TransitionTo(Completed),

               
                When(OrderStatusFailed)
                    .Then(context =>
                    {
                       
                    })
                    .Publish(context => new PaymentFailEvent(context.Message.OrderId))
                    .TransitionTo(Faulted)
            );
        }
    }
}
