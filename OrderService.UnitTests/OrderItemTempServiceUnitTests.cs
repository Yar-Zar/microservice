using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Application.Common.Cache;
using OrderService.Application.DTOs;
using OrderService.Application.Services;
using OrderService.Application.Validations.OrderItems;
using OrderService.Application.Validations.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.UnitTests;

public class OrderItemTempServiceUnitTests
{
    private readonly Mock<IRedisHashProvider> _mockCache;
    private readonly Mock<ILogger<OrderItemTempService>> _mockLogger;
    private readonly OrderItemDTOValidator _createValidator;
    private readonly UpdateOrderItemValidator _updateValidator;

    private readonly OrderItemTempService _tempService;

    public OrderItemTempServiceUnitTests()
    {
        _mockCache = new Mock<IRedisHashProvider>();
        _mockLogger = new Mock<ILogger<OrderItemTempService>>();

        _createValidator = new OrderItemDTOValidator();
        _updateValidator = new UpdateOrderItemValidator();

        _tempService = new OrderItemTempService(
            _mockCache.Object,
            _createValidator,
            _updateValidator,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetOrderItemTemp_ShouldReturnItems_WhenCacheHasData()
    {
        // Arrange
        var orderId = "TEMP-001";
        var expectedItems = new List<OrderItemDTO>
        {
            new OrderItemDTO { ProductId = 1, Quantity = 2 }
        };

        _mockCache.Setup(c => c.GetAllAsync<OrderItemDTO>(orderId))
                  .ReturnsAsync(expectedItems);

        // Act
        var result = await _tempService.GetOrderItemTemp(orderId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].ProductId.Should().Be(1);
        _mockCache.Verify(c => c.GetAllAsync<OrderItemDTO>(orderId), Times.Once);
    }

    [Fact]
    public async Task AddOrderItemTemp_ShouldReturnResponseAndItems_WhenItemNotExists()
    {
        // Arrange
        var request = new OrderItemDTO { OrderId = "TEMP-001", ProductId = 1, Quantity = 3, UnitPrice = 100 };

        _mockCache.Setup(c => c.CheckExistAsync<OrderItemDTO>(It.IsAny<string>(), It.IsAny<System.Linq.Expressions.Expression<Func<OrderItemDTO, bool>>>()))
                  .ReturnsAsync(false);

        _mockCache.Setup(c => c.SaveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<OrderItemDTO>(), It.IsAny<TimeSpan>()))
                  .ReturnsAsync(true);

        _mockCache.Setup(c => c.GetAllAsync<OrderItemDTO>(request.OrderId))
                  .ReturnsAsync(new List<OrderItemDTO> { request });

        // Act
        var (response, items) = await _tempService.AddOrderItemTemp(request, CancellationToken.None);

        // Assert (စစ်ဆေးခြင်း)
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();

        // 1. Items တွေကိုပါ စစ်ဆေးရန် (Test နာမည်နဲ့ ကိုက်ညီစေရန်)
        items.Should().NotBeNull();
        items.Should().ContainSingle();
        items.Should().Contain(x => x.ProductId == request.ProductId);

        // 2. Cache Interactions များကို Verify လုပ်ရန် (Mock တွေကို အမှန်တကယ် ခေါ်သွားခြင်း ရှိမရှိ)
        _mockCache.Verify(c => c.CheckExistAsync<OrderItemDTO>(It.IsAny<string>(), It.IsAny<System.Linq.Expressions.Expression<Func<OrderItemDTO, bool>>>()), Times.Once);
        _mockCache.Verify(c => c.SaveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<OrderItemDTO>(), It.IsAny<TimeSpan>()), Times.Once);
        _mockCache.Verify(c => c.GetAllAsync<OrderItemDTO>(request.OrderId), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderItemTemp_ShouldReturnResponseAndItems_WhenItemExists()
    {
        // Arrange
        var request = new OrderItemDTO { Id=1,OrderId = "TEMP-001", ProductId = 1, Quantity = 5, UnitPrice = 100 };

        _mockCache.Setup(c => c.GetAsync<OrderItemDTO>(request.OrderId, request.ProductId.ToString()))
                  .ReturnsAsync(request);

        _mockCache.Setup(c => c.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<OrderItemDTO>(), It.IsAny<TimeSpan>()))
                  .ReturnsAsync(true);

        _mockCache.Setup(c => c.GetAllAsync<OrderItemDTO>(request.OrderId))
                  .ReturnsAsync(new List<OrderItemDTO> { request });

        // Act
        var (response, items) = await _tempService.UpdateOrderItemTemp(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be("200");

        
        _mockCache.Verify(c => c.GetAsync<OrderItemDTO>(request.OrderId, request.ProductId.ToString()), Times.Once);
        _mockCache.Verify(c => c.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<OrderItemDTO>(), It.IsAny<TimeSpan>()), Times.Once);
        _mockCache.Verify(c => c.GetAllAsync<OrderItemDTO>(request.OrderId), Times.Once);
    }

    [Fact]
    public async Task DeleteOrderItemTemp_ShouldReturnResponseAndItems_WhenItemExists()
    {
        // Arrange
        var orderId = "TEMP-001";
        var productId = "1";
        var existingItem = new OrderItemDTO { ProductId = 1, Quantity = 2 };

        _mockCache.Setup(c => c.GetAsync<OrderItemDTO>(orderId, productId))
                  .ReturnsAsync(existingItem);

        _mockCache.Setup(c => c.DeleteFieldAsync(orderId, productId))
                  .ReturnsAsync(true);

        _mockCache.Setup(c => c.GetAllAsync<OrderItemDTO>(orderId))
                  .ReturnsAsync(new List<OrderItemDTO>());

        // Act
        var (response, items) = await _tempService.DeleteOrderItemTemp(orderId, productId);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be("200");

        items.Should().NotBeNull();
        items.Should().BeEmpty();

        // Cache Interactions များကို Verify လုပ်ရန်
        _mockCache.Verify(c => c.GetAsync<OrderItemDTO>(orderId, productId), Times.Once);
        _mockCache.Verify(c => c.DeleteFieldAsync(orderId, productId), Times.Once);
        _mockCache.Verify(c => c.GetAllAsync<OrderItemDTO>(orderId), Times.Once);
    }
}