namespace OrderService.Application.Validations.OrderItems
{
    public class OrderItemDTOValidator<T> : AbstractValidator<T> where T : class
    {
        public OrderItemDTOValidator(Func<T, OrderItemDTO> selector)
        {
            RuleFor(oi => selector(oi).ProductId)
                .NotEmpty().WithMessage("ProductId is required.");

            RuleFor(oi => selector(oi).Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

            RuleFor(oi => selector(oi).UnitPrice)
                .GreaterThan(0).WithMessage("UnitPrice must be greater than zero.");
        }
    }
}
