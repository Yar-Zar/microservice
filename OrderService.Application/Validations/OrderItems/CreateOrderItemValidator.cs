
namespace OrderService.Application.Validations.OrderItems;

public class CreateOrderItemValidator : AbstractValidator<OrderItemDTO>
{
    public CreateOrderItemValidator()
    {
        Include(new OrderItemDTOValidator<OrderItemDTO>(x=> x));
    }
}
