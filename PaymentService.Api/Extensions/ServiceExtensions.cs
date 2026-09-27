namespace PaymentService.Api.Extensions;
public static class ServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHealthChecks();
        services.AddHttpContextAccessor();
        // Add MassTransit with RabbitMQ
       services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMQSettings:Host"] ?? "localhost";
                var username = configuration["RabbitMQSettings:Username"] ?? "guest";
                var password = configuration["RabbitMQSettings:Password"] ?? "guest";

                cfg.Host(host, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });
            });
        });
        //services.AddAuthorization();
        //services.AddAuthentication();

        return services;
    }
}
