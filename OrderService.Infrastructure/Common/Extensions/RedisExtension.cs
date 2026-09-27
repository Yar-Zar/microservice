namespace OrderService.Infrastructure.Common.Extensions;

public static class RedisExtension
{
    public static IServiceCollection AddRedisConfiguration(this IServiceCollection services, IConfiguration config)
    {
        // Register Redis Connection
        var redisConnection = ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")!);
        services.AddSingleton<IConnectionMultiplexer>(redisConnection);

        // 2. Built-in IDistributedCache connecting with StackExchangeRedis 
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = config.GetConnectionString("Redis");
            options.InstanceName = "OrderAPI_"; 
        });
        return services;
    }
}
