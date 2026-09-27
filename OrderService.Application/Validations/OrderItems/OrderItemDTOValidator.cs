namespace OrderService.Application.Validations.OrderItems
{
    public class OrderItemDTOValidator : AbstractValidator<OrderItemDTO>
    {
        public OrderItemDTOValidator()
        {
            RuleFor(oi => oi.ProductId)
                .NotEmpty().WithMessage("ProductId is required.");

            RuleFor(oi => oi.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

            RuleFor(oi => oi.UnitPrice)
                .GreaterThan(0).WithMessage("UnitPrice must be greater than zero.");
        }
    }
}
