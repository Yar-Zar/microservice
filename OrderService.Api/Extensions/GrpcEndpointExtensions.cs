using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using OrderService.Application.GrpcService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Common.Extensions;

public static class GrpcEndpointExtensions
{
    public static IEndpointRouteBuilder MapCustomGrpcServices(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGrpcService<OrderGrpcService>();
        //add next gRPC service here
        return endpoints;
    }
}
