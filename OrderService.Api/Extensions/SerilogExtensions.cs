namespace OrderService.Api.Extensions;
public static class SerilogExtensions
{
    public static IHostBuilder ConfigureSerilog(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console());

        return hostBuilder;
    }
}

