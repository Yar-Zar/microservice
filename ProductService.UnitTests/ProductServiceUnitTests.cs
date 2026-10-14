namespace ProductService.UnitTests;

public class ProductServiceUnitTests
{
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CreateProductCommandHandler>> _loggerMock;
    private readonly Mock<ILogger<UpdateProductCommandHandler>> _loggerUpdateMock;

    private readonly CreateProductCommandHandler _handler;
    private readonly UpdateProductCommandHandler _handlerUpdate;
    private readonly GetProductsQueryHandler _getQueryHandler;
    private readonly GetProductsForDropdownQueryHandler _getDropdownQueryHandler;

    public ProductServiceUnitTests()
    {
        _productRepoMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CreateProductCommandHandler>>();
        _loggerUpdateMock = new Mock<ILogger<UpdateProductCommandHandler>>();

        _handler = new CreateProductCommandHandler(_productRepoMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);
        _handlerUpdate = new UpdateProductCommandHandler(_productRepoMock.Object, _unitOfWorkMock.Object, _loggerUpdateMock.Object);
        _getQueryHandler = new GetProductsQueryHandler(_productRepoMock.Object);
        _getDropdownQueryHandler = new GetProductsForDropdownQueryHandler(_productRepoMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateProductAndReturnSuccess()
    {
        // Arrange
        var command = new CreateProductCommand(
            new ProductDTO
            {
                Name = "Test Product",
                Price = 99.99m,
                Stock = 50,
                Description = "Test Product Description"
            }
        );

        _productRepoMock
            .Setup(x => x.CheckExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Callback<Func<Task>, CancellationToken>((func, ct) => func());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be("200");
        result.Message.Should().Be("Product created successfully.");

        _productRepoMock.Verify(x => x.AddAsync(It.IsAny<Product>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateName_ShouldThrowDuplicateException()
    {
        // Arrange
        var command = new CreateProductCommand(
            new ProductDTO
            {
                Name = "Existing Product",
                Price = 99.99m,
                Stock = 50,
                Description = "Test Product Description"
            }
        );

        _productRepoMock
            .Setup(x => x.CheckExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DuplicateException>()
            .WithMessage("Product with the same name already exists.");

        _productRepoMock.Verify(x => x.AddAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldUpdateProductAndReturnSuccess()
    {
        // Arrange
        var productId = 1;
        var command = new UpdateProductCommand(
            productId,
            new ProductDTO
            {
                Name = "Updated Name",
                Price = 120.00m,
                Stock = 100,
                Description = "Updated Description"
            }
        );

        var existingProduct = new Product { Id = productId, Name = "Old Name" };

        _productRepoMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _productRepoMock
            .Setup(x => x.CheckExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Callback<Func<Task>, CancellationToken>((func, ct) => func());

        // Act
        var result = await _handlerUpdate.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be("200");

        _unitOfWorkMock.Verify(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateProductCommand(
            99,
            new ProductDTO
            {
                Name = "Any Name",
                Price = 10.00m,
                Stock = 10
            }
        );

        _productRepoMock
            .Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null!);

        // Act
        Func<Task> act = async () => await _handlerUpdate.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Product with ID 99 was not found.");
    }

    [Fact]
    public async Task UpdateProduct_WithDuplicateName_ShouldThrowDuplicateException()
    {
        // Arrange
        var productId = 1;
        var command = new UpdateProductCommand(
            productId,
            new ProductDTO
            {
                Name = "Duplicate Name",
                Price = 10.00m,
                Stock = 10
            }
        );

        var existingProduct = new Product { Id = productId, Name = "Original Name" };

        _productRepoMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _productRepoMock
            .Setup(x => x.CheckExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _handlerUpdate.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DuplicateException>()
            .WithMessage("Another product with the same name already exists.");
    }

    [Fact]
    public async Task GetProductsQueryHandler_ShouldReturnListOfProductDTOs()
    {
        // Arrange
        var expectedProducts = new List<ProductDTO>
        {
            new ProductDTO { Id = 1, Name = "Product 1" },
            new ProductDTO { Id = 2, Name = "Product 2" }
        };

        var query = new GetProductsQuery(
            new AppFilter
            {
                CurrentPageNo = 1,
                CurrentRowLimit = 10,
                SearchInput = string.Empty
            }
        );

        _productRepoMock
            .Setup(x => x.GetAllAsync<ProductDTO>(query.Filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProducts);

        // Act
        var result = await _getQueryHandler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedProducts);
    }

    [Fact]
    public async Task GetProductsForDropdownQueryHandler_ShouldReturnDropdownList()
    {
        // Arrange
        var expectedDropdowns = new List<DropdownDto>
        {
            new DropdownDto(1, "Product 1")
        };

        _productRepoMock
            .Setup(x => x.GetProductsForDropdownAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDropdowns);

        var query = new GetProductsForDropdownQuery();

        // Act
        var result = await _getDropdownQueryHandler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDropdowns);
    }
}