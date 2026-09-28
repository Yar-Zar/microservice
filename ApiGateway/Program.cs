using ApiGateway.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register Gateway Services via Extension Method
builder.Services.AddGatewayServices(builder.Configuration);

var app = builder.Build();

// Configure Middleware Pipeline via Extension Method
app.UseGatewayPipeline();

app.Run();