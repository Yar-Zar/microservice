namespace Shared.Contracts.Events;
public record CreatePaymentEvent(string OrderId);
public record PaymentCompletedEvent(string OrderId);
public record PaymentFailEvent(string OrderId);
public record OrderStatusSuccessedEvent(string OrderId);
public record OrderStatusFailedEvent(string OrderId);
