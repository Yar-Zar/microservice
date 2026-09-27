namespace OrderService.Application.Validations.Orders;
public class UpdateOrderValidator : AbstractValidator<OrderDTO>
{
    public UpdateOrderValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Order ID is required.");
        Include(new OrderDTOValidator<OrderDTO>(x => x));
    }
}

