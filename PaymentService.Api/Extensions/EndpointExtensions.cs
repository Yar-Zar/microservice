namespace PaymentService.Api.Extensions;
public static class EndpointExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {

        app.MapPaymentEndpoints();

        return app;
    }
}

