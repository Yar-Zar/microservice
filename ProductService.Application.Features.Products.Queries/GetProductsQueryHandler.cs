namespace ProductService.Application.Features.Products.Queries;
public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IEnumerable<ProductDTO>>
{
    private readonly IProductRepository _productQueryRepository;
    private readonly ILogger<GetProductsQueryHandler> _logger;

    public GetProductsQueryHandler(IProductRepository productQueryRepository, ILogger<GetProductsQueryHandler> logger)
    {
        _productQueryRepository = productQueryRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductDTO>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        _logger.LogInformation("Handling GetProductsQuery with Filter {@Filter}", request.Filter);

        try
        {
            var rawResults = await _productQueryRepository.GetAllAsync<ProductDTO>(request.Filter, ct);
            var results = rawResults?.ToList() ?? new List<ProductDTO>();

            _logger.LogInformation("Retrieved {Count} products for filter {@Filter}", results.Count, request.Filter);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching products for filter {@Filter}", request.Filter);
            throw;
        }
    }
}