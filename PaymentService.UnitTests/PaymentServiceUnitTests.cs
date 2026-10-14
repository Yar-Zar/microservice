namespace PaymentService.UnitTests;

public class PaymentServiceUnitTests
{
    private readonly Mock<IPaymentRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<Payment_Service>> _loggerMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly PaymentDTOValidator _validatorMock;
    private readonly UpdatePaymentDTOValidator _updateValidatorMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly Mock<OrderGrpcService.OrderGrpcServiceClient> _orderClientMock = new();

    private readonly Payment_Service _sut;

    public PaymentServiceUnitTests()
    {
        // Unit of Work တွင် Action Delegate ကို ချက်ချင်း Execute လုပ်ပေးရန် Setup ပြုလုပ်ခြင်း
        _unitOfWorkMock
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>(async (action, ct) => await action());
        _validatorMock = new PaymentDTOValidator();
        _updateValidatorMock = new UpdatePaymentDTOValidator();
        _sut = new Payment_Service(
            _repoMock.Object,
            _publishEndpointMock.Object,
            _orderClientMock.Object,
            _httpContextAccessorMock.Object,
            _validatorMock,
            _updateValidatorMock,
            _loggerMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task SavePaymentAsync_WhenValidDtoAndNotPaid_ReturnsSuccessResponse()
    {
        // Arrange
        var dto = new PaymentDTO
        {
            Id = 1,
            OrderId = "ORD-001",
            Amount = 150,
            Currency = "USD",
            PaymentStatus = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.AddAsync(It.IsAny<Payment>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.SavePaymentAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be("200");
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task SavePaymentAsync_WhenPaidAndGrpcSuccess_ReturnsSuccessResponse()
    {
        // Arrange
        var dto = new PaymentDTO
        {
            Id = 1,
            OrderId = "ORD-001",
            Amount = 250,
            Currency = "USD",
            PaymentStatus = "Paid",
            CreatedAt = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.AddAsync(It.IsAny<Payment>()))
            .Returns(Task.CompletedTask);

        var grpcResponse = new UpdateOrderStatusResponse { Success = true };
        _orderClientMock
            .Setup(client => client.UpdateOrderStatusAsync(
                It.IsAny<UpdateOrderStatusRequest>(),
                null, null, It.IsAny<CancellationToken>()))
            .Returns(new AsyncUnaryCall<UpdateOrderStatusResponse>(
                Task.FromResult(grpcResponse),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { }));

        // Act
        var result = await _sut.SavePaymentAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _orderClientMock.Verify(client => client.UpdateOrderStatusAsync(It.IsAny<UpdateOrderStatusRequest>(), null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenPaymentExists_DeletesAndReturnsSuccessResponse()
    {
        // Arrange
        int paymentId = 1;
        var existingPayment = new Payment { Id = paymentId, OrderId = "ORD-001" };

        _repoMock.Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        _repoMock.Setup(r => r.DeleteAsync(existingPayment))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteAsync(paymentId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("deleted successfully");
        _repoMock.Verify(r => r.DeleteAsync(existingPayment), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenPaymentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        int paymentId = 99;
        _repoMock.Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(paymentId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdatePaymentAsync_WhenIdsDoNotMatch_ThrowsInvalidOperationException()
    {
        // Arrange
        int urlId = 1;
        var dto = new PaymentDTO { Id = 2 }; // Mismatched ID

        // Act
        Func<Task> act = async () => await _sut.UpdatePaymentAsync(urlId, dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetByIdAsync_WhenPaymentExists_ReturnsPaymentDTO()
    {
        // Arrange
        int paymentId = 1;
        var entity = new Payment { Id = paymentId, OrderId = "ORD-001", Amount = 200, PaymentStatus = "Paid" };

        _repoMock.Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        var result = await _sut.GetByIdAsync(paymentId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(paymentId);
        result.OrderId.Should().Be("ORD-001");
    }

    [Fact]
    public async Task GetByIdAsync_WhenPaymentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        int paymentId = 10;
        _repoMock.Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        // Act
        Func<Task> act = async () => await _sut.GetByIdAsync(paymentId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdatePaymentStatusAsync_WhenUpdateFailsOrThrows_PublishesPaymentFailEvent()
    {
        // Arrange
        string orderId = "ORD-001";
        string status = "Failed";

        _repoMock.Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null); // Will trigger NotFoundException inside transaction

        _publishEndpointMock
            .Setup(p => p.Publish(It.IsAny<PaymentFailEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        Func<Task> act = async () => await _sut.UpdatePaymentStatusAsync(orderId, status, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _publishEndpointMock.Verify(p => p.Publish(It.IsAny<PaymentFailEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}