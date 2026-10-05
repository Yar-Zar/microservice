namespace OrderService.Api.Extensions;
public static class SerilogExtensions
{
    public static IHostBuilder ConfigureSerilog(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog((context, configuration) => configuration
          .ReadFrom.Configuration(context.Configuration)
          .Enrich.FromLogContext()
          .Enrich.WithCorrelationId()
          .Enrich.WithEnvironmentUserName()
          .Enrich.WithThreadId()
          .Enrich.WithProcessId());

        return hostBuilder;
    }
}

