
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();
try
{
    Log.Information("Order Api Starting Up...");
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.ConfigureSerilog();
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddApiServices(builder.Configuration);
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddRedisConfiguration(builder.Configuration);
    builder.Services.AddInfrastructureServices(builder.Configuration);
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.ConfigurePipeline();
    app.MapCustomGrpcServices();
    app.MapEndpoints();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed!");
}
finally
{
    Log.CloseAndFlush();
}



