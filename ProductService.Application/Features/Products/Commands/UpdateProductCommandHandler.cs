namespace ProductService.Application.Features.Products.Commands;
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ResponseModel>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ResponseModel> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Start UpdateProductCommand for ProductId {ProductId}", request.Id);

        ResponseModel response = new ResponseModel
        {
            IsSuccess = true,
            Message = "Product updated successfully.",
            StatusCode = "200"
        };

        try
        {
            // Check Data Existence Before Update
            var product = await _productRepository.GetByIdAsync(request.Id, ct);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} was not found.", request.Id);
                throw new NotFoundException($"Product with ID {request.Id} was not found.");
            }

            // check for duplicate name before updating
            var existingProduct = await _productRepository.CheckExistsAsync(x => x.Name == request.Product.Name && x.Id != request.Id);
            if (existingProduct)
            {
                _logger.LogWarning("Duplicate product name detected for ProductId {ProductId}; Name: {ProductName}", request.Id, request.Product.Name);
                throw new DuplicateException("Another product with the same name already exists.");
            }

            _logger.LogDebug("Applying updates to ProductId {ProductId}", request.Id);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                request.Product.Adapt(product);
            }, ct);

            _logger.LogInformation("Product {ProductId} updated successfully.", request.Id);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", request.Id);
            throw;
        }
    }
}

