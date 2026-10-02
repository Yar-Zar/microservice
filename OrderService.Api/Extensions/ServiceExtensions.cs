using OrderService.Application.Consumer;
using OrderService.Application.Consummer;
using Shared.GrpcContracts.Payment;
using Shared.GrpcContracts.Product;

namespace OrderService.Api.Extensions;
public static class ServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHealthChecks();
        services.AddHttpContextAccessor();
        services.AddMassTransit(x =>
        {

            // x.AddConsumers(typeof(Program).Assembly);
            x.AddConsumers(typeof(PaymentCompletedConsumer).Assembly);
            x.AddConsumers(typeof(PaymentFailedConsumer).Assembly);
            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMQ:Host"] ?? "localhost";
                var username = configuration["RabbitMQ:Username"] ?? "guest";
                var password = configuration["RabbitMQ:Password"] ?? "guest";

                cfg.Host(host, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                
                cfg.ConfigureEndpoints(context);
            });
        });
        #region gRPC Services
        services.AddGrpc();
        services.AddGrpcClient<PaymentGrpcService.PaymentGrpcServiceClient>(options =>
        {
            var paymentServiceUrl = configuration["ServiceUrls:PaymentService"];
            options.Address = new Uri(paymentServiceUrl); // Payment Service URL
        });
        services.AddGrpcClient<ProductGrpcService.ProductGrpcServiceClient>(options =>
        {
            var productServiceUrl = configuration["ServiceUrls:ProductService"];
            options.Address = new Uri(productServiceUrl); // Product Service URL
        });
        #endregion
        //services.AddAuthorization();
        //services.AddAuthentication();

        return services;
    }
}
