namespace OrderService.Api.Extensions;
public static class ServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHealthChecks();
        services.AddHttpContextAccessor();
        //services.AddAuthorization();
        //services.AddAuthentication();

        return services;
    }
}
