namespace OrderService.Application.Validations.Orders;
public class OrderDTOValidator : AbstractValidator<OrderDTO>
{
    public OrderDTOValidator()
    {
        RuleFor(o => o.CustomerId)
                .NotEmpty().WithMessage("CustomerId is required.")
                .MaximumLength(100).WithMessage("CustomerId must not exceed 100 characters.");

        RuleFor(o => o.OrderDate)
            .NotEmpty().WithMessage("OrderDate is required.")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("OrderDate cannot be in the future.");

        RuleFor(o => o.TotalAmount)
            .GreaterThan(0).WithMessage("TotalAmount must be greater than zero.");

        RuleFor(o => o.CreatedBy)
            .MaximumLength(50).WithMessage("CreatedBy must not exceed 50 characters.")
            .When(o => !string.IsNullOrEmpty(o.CreatedBy));

        RuleFor(o => o.OrderItems)
            .NotEmpty().WithMessage("An order must contain at least one item.");


        RuleForEach(o => o.OrderItems)
            .SetValidator(new OrderItemDTOValidator());
    }
}

