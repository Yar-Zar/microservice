namespace OrderService.Infrastructure.Common.Extensions;
public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Entity Framework Core (DbContext) Config
        services.AddDbContext<OrderDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(OrderDbContext).Assembly.FullName)));

        // 2. Dapper Config
        services.AddScoped<QueryHelper>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IRedisHashProvider, RedisHashProvider>();
        services.AddScoped<IOrderRepository, OrderRepository>();



        return services;
    }
}
