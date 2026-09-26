namespace ProductsService.Api.Extensions;
public static class EndpointExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {

        app.MapProductEndpoints();

        return app;
    }
}

