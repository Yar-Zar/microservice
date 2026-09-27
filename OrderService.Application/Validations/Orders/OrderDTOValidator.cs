namespace OrderService.Application.Validations.Orders;
public class OrderDTOValidator<T> : AbstractValidator<T> where T : class
{
    public OrderDTOValidator(Func<T, OrderDTO> selector)
    {
        RuleFor(o => selector(o).CustomerId)
                .NotEmpty().WithMessage("CustomerId is required.")
                .MaximumLength(100).WithMessage("CustomerId must not exceed 100 characters.");

        RuleFor(o => selector(o).OrderDate)
            .NotEmpty().WithMessage("OrderDate is required.")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("OrderDate cannot be in the future.");

        RuleFor(o => selector(o).TotalAmount)
            .GreaterThan(0).WithMessage("TotalAmount must be greater than zero.");

        RuleFor(o => selector(o).CreatedBy)
            .MaximumLength(50).WithMessage("CreatedBy must not exceed 50 characters.")
            .When(o => !string.IsNullOrEmpty(selector(o).CreatedBy));

        RuleFor(o => selector(o).OrderItems)
            .NotEmpty().WithMessage("An order must contain at least one item.");


        RuleForEach(o => selector(o).OrderItems)
            .SetValidator(new OrderItemDTOValidator<OrderItemDTO>(item => item));
    }
}

