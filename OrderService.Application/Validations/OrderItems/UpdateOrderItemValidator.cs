using OrderService.Application.Validations.Orders;

namespace OrderService.Application.Validations.OrderItems;

internal class UpdateOrderItemValidator : AbstractValidator<OrderItemDTO>
{
    public UpdateOrderItemValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Item ID is required.");
        Include(new OrderItemDTOValidator<OrderItemDTO>(x => x));
    }
}
