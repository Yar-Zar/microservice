namespace ProductService.Application.Features.Products.Queries;
public class GetProductsForDropdownQueryHandler : IRequestHandler<GetProductsForDropdownQuery, IEnumerable<DropdownDto>>
{
    private readonly IProductRepository _productQueryRepository;

    public GetProductsForDropdownQueryHandler(IProductRepository productQueryRepository)
    {
        _productQueryRepository = productQueryRepository;
    }

    public async Task<IEnumerable<DropdownDto>> Handle(GetProductsForDropdownQuery request, CancellationToken ct)
    {
        return await _productQueryRepository.GetProductsForDropdownAsync(ct);
    }
}

