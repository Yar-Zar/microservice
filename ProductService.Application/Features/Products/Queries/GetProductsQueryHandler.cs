namespace ProductService.Application.Features.Products.Queries;
public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IEnumerable<ProductDTO>>
{
    private readonly IProductRepository _productQueryRepository;

    public GetProductsQueryHandler(IProductRepository productQueryRepository)
    {
        _productQueryRepository = productQueryRepository;
    }

    public async Task<IEnumerable<ProductDTO>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var rawResults = (await _productQueryRepository.GetAllAsync<ProductDTO>(request.Filter, ct));

        return rawResults;
    }
}

