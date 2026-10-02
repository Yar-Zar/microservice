using Grpc.Core;
using Shared.GrpcContracts.Payment;
using Shared.GrpcContracts.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Shared.GrpcContracts.Product.ProductGrpcService;

namespace ProductService.Application.GrpcServices;

public class ProductGrpcService:ProductGrpcServiceBase
{
    private readonly IProductRepository _productRepo;

    public ProductGrpcService(IProductRepository productRepo)
    {
        _productRepo = productRepo;
    }
    public override async Task<ProductListResponse> GetProductsByIds(
       RequestProductIds request,
       ServerCallContext context)
    {

        var products = await _productRepo.GetByIdsAsync(request.ProductIds, context.CancellationToken);
        var response = new ProductListResponse();
        response.Items.AddRange(products);
        return response;
    }
}
