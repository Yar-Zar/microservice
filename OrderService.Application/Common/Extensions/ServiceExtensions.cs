namespace OrderService.Application.Common.Extensions;
public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
       
        #region Mapster Configuration
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
        #endregion

        #region Fluent Validation & Pipeline Behavior
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        #endregion

        services.AddScoped<IOrderService, Order_Service>();
        services.AddScoped<IOrderItemTempService, OrderItemTempService>();

        return services;
    }
}

