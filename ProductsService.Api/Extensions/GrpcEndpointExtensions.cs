using ProductService.Application.GrpcServices;

namespace ProductsService.Api.Extensions;

public static class GrpcEndpointExtensions
{
    public static IEndpointRouteBuilder MapCustomGrpcServices(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGrpcService<ProductGrpcService>();
        //add next gRPC service here
        return endpoints;
    }
}