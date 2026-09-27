using ProductService.Domain.Entities;

namespace ProductService.Application.Features.Products.Commands;
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ResponseModel>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ResponseModel> Handle(CreateProductCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Handle: creating product {ProductName}", request.Product?.Name);

        ResponseModel response = new ResponseModel
        {
            IsSuccess = true,
            Message = "Product created successfully.",
            StatusCode = "200"
        };

        try
        { // check duplicate product name
            var existingProduct = await _productRepository.CheckExistsAsync(x => x.Name == request.Product.Name);
            if (existingProduct)
            {
                _logger.LogWarning("Duplicate product creation attempt for {ProductName}", request.Product?.Name);
                throw new DuplicateException("Product with the same name already exists.");
            }
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var product = request.Product.Adapt<Product>();

                    _logger.LogDebug("Adding product entity to repository {@Product}", product);
                    await _productRepository.AddAsync(product);

                }, ct);
            // Log created product id if available after save
            _logger.LogInformation("Product created successfully. ProductId: {ProductId}, Name: {ProductName}", request.Product.Id, request.Product.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating product {ProductName}", request.Product?.Name);
            throw;
        }

        return response;
    }
}

