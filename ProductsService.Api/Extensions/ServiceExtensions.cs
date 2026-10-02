using Shared.GrpcContracts.Order;

namespace ProductsService.Api.Extensions;
public static class ServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHealthChecks();
        #region gRPC Services


        services.AddGrpc();
        services.AddGrpcClient<OrderGrpcService.OrderGrpcServiceClient>(options =>
        {
            var orderServiceUrl = configuration["ServiceUrls:OrderService"];
            options.Address = new Uri(orderServiceUrl);
        });
        #endregion
        //services.AddAuthorization();
        //services.AddAuthentication();

        return services;
    }
}
