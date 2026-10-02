using PaymentService.Application.GrpcServices;

namespace PaymentService.Api.Extensions;

public static class GrpcEndpointExtensions
{
    public static IEndpointRouteBuilder MapCustomGrpcServices(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGrpcService<PaymentGrpcService>();
        //add next gRPC service here
        return endpoints;
    }
}