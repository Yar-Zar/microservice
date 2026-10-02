using PaymentService.Application.Common.StateMachines;
using PaymentService.Application.Consumers;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Data;
using Shared.GrpcContracts.Order;

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
            // 1. Saga State Machine 
            x.AddSagaStateMachine<PaymentStateMachine, PaymentState>()
     .EntityFrameworkRepository(r =>
     {
         r.ExistingDbContext<PaymentDbContext>();
         r.UseSqlServer();
         //r.AddDbContext<DbContext, PaymentDbContext>((provider, builder) =>
         //{
         //    builder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
         //});
     });
            x.AddConsumers(typeof(OrderStatusFailedConsumer).Assembly);
            x.AddConsumers(typeof(OrderStatusSuccessedConsumer).Assembly);
            // 2. Entity Framework Outbox 
            //x.AddEntityFrameworkOutbox<PaymentDbContext>(o =>
            //{
            //    o.QueryDelay = TimeSpan.FromSeconds(1);
            //    o.UseSqlServer();
            //    o.UseBusOutbox();
            //});

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
                cfg.UseInMemoryOutbox(context);
                cfg.ConfigureEndpoints(context);
            });
        });
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
