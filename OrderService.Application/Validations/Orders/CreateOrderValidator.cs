namespace OrderService.Application.Validations.Orders;
public class CreateOrderValidator : AbstractValidator<OrderDTO>
{
    public CreateOrderValidator()
    {
        Include(new OrderDTOValidator<OrderDTO>(x => x));
    }
}

