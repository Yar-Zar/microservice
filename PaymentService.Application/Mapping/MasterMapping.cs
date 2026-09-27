namespace PaymentService.Application.Mapping;
public class MasterMapping
{
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Payment, PaymentDTO>().TwoWays();
    }
}

