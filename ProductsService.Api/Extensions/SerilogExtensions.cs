

public static class SerilogExtensions
{
    public static IHostBuilder ConfigureSerilog(
        this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
        {
            if (context.HostingEnvironment.IsEnvironment("Testing"))
            {
                // Integration Testing: Console only.
                // Do not initialize the SQL Server logging sink.
                loggerConfiguration
                    .MinimumLevel.Information()
                    .Enrich.FromLogContext()
                    .WriteTo.Console();
            }
            else
            {
                // Development / Production:
                // Load configured sinks from appsettings.json.
                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext();
            }
        });

        return hostBuilder;
    }
}