var builder = WebApplication.CreateBuilder(args);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Host.ConfigureSerilog();
var app = builder.Build();
SelfLog.Enable(Console.Out);
try
{
    Log.Information("Product Api Starting Up...");


    // Configure the HTTP request pipeline.
    app.ConfigurePipeline();
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



