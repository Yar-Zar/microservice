namespace OrderService.Application.Mapping;
public class MasterMapping
{
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Order, OrderDTO>().TwoWays();
        config.NewConfig<OrderItem, OrderItemDTO>().TwoWays();
    }
}

