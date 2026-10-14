namespace OrderService.UnitTests;
public class OrderServiceUnitTests
{
    private readonly Mock<IRedisHashProvider> _mockCache;
    private readonly Mock<IOrderRepository> _mockOrderRepo;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IValidator<OrderDTO>> _mockValidator;
    private readonly Mock<UpdateOrderValidator> _mockUpdateValidator;
    private readonly Mock<ILogger<Order_Service>> _mockLogger;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Mock<PaymentGrpcService.PaymentGrpcServiceClient> _mockPaymentClient;
    private readonly Mock<ProductGrpcService.ProductGrpcServiceClient> _mockProductClient;

    private readonly Order_Service _orderService;

    public OrderServiceUnitTests()
    {
        _mockCache = new Mock<IRedisHashProvider>();
        _mockOrderRepo = new Mock<IOrderRepository>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockValidator = new Mock<IValidator<OrderDTO>>();
        _mockUpdateValidator = new Mock<UpdateOrderValidator>();
        _mockLogger = new Mock<ILogger<Order_Service>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockPaymentClient = new Mock<PaymentGrpcService.PaymentGrpcServiceClient>();
        _mockProductClient = new Mock<ProductGrpcService.ProductGrpcServiceClient>();

        // Service ကို Mock dependencies များနှင့်တကွ တည်ဆောက်ခြင်း
        _orderService = new Order_Service(
            _mockCache.Object,
            _mockOrderRepo.Object,
            _mockHttpContextAccessor.Object,
            _mockValidator.Object,
            _mockUpdateValidator.Object,
            _mockLogger.Object,
            _mockUnitOfWork.Object,
            _mockPublishEndpoint.Object,
            _mockPaymentClient.Object,
            _mockProductClient.Object
        );
    }

    [Fact]
    public async Task SaveOrderAsync_ShouldReturnSuccessResponse_WhenDtoIsValid()
    {
        // Arrange
        var dto = new OrderDTO
        {
            Id = "ORD-001",
            CustomerId = "CUST-001",
            TotalAmount = 250.0m,
            OrderDate = DateTime.UtcNow,
            Status = "Pending",
            OrderItems = new List<OrderItemDTO>
    {
        new OrderItemDTO
        {
            OrderId= "ORD-001",
            ProductId = 1,
            Quantity = 2,
            UnitPrice = 75.0m
        }
    }
        };
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<OrderDTO>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mockOrderRepo.Setup(repo => repo.GetByIdAsync(dto.Id,It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Order)null);

        _mockOrderRepo.Setup(repo => repo.AddAsync(It.IsAny<Order>()))
                      .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(uow => uow.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
                       .Callback<Func<Task>, CancellationToken>(async (action, ct) =>
                       {
                           await action();
                       }).Returns(Task.CompletedTask);

        // Act
        var result = await _orderService.SaveOrderAsync(dto, CancellationToken.None);

        // Assert
         result.IsSuccess.Should().BeTrue();
        _mockOrderRepo.Verify(repo => repo.GetByIdAsync(dto.Id, It.IsAny<CancellationToken>()), Times.Once);
        _mockOrderRepo.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenOrderExists()
    {
        // Arrange
        var orderId = "ORD-001";
        var mockOrder = new Order
        {
            Id = orderId,
            CustomerId = "CUST-001",
            TotalAmount = 250.0m,
            OrderDate = DateTime.UtcNow,
            Status = "Pending"
        };

        _mockOrderRepo.Setup(repo => repo.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(mockOrder);

        // Act
        var result = await _orderService.GetByIdAsync(orderId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockOrderRepo.Verify(repo => repo.GetByIdAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnSuccess_WhenOrderExists()
    {
        var orderId = "ORD-001";
        var mockOrder = new Order
        {
            Id = orderId,
            CustomerId = "CUST-001",
            TotalAmount = 250.0m,
            OrderDate = DateTime.UtcNow,
            Status = "Pending"
        };


        _mockOrderRepo.Setup(repo => repo.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(mockOrder);

        var result = await _orderService.DeleteAsync(orderId, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllOrderAsync_ShouldReturnList_WhenFilterIsValid()
    {
        // Arrange
        var filter = new AppFilter { CurrentPageNo = 1, CurrentRowLimit = 10 };
        _mockOrderRepo.Setup(repo => repo.GetAllAsync<Order>(filter, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<Order>());

        // Act
        var result = await _orderService.GetAllOrderAsync(filter, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }
}