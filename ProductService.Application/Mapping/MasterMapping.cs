namespace ProductService.Application.Mapping;
public class MasterMapping
{
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductDTO>().TwoWays();
    }
}

