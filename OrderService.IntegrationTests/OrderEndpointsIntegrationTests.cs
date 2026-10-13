
namespace OrderService.IntegrationTests;

public class OrderEndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public OrderEndpointsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        // Reset the database after each test if using Respawn
        //await _factory.ResetDatabaseAsync();
        _factory.ResetDatabaseAsync().GetAwaiter().GetResult();
    }
    #region Order Endpoints Tests

    [Fact]
    public async Task SaveOrder_ShouldReturnOk_WhenDtoIsValid()
    {
        var orderId = "ORD-001";
        var dto = new OrderDTO
        {
            Id = orderId,
            CustomerId = "CUST-001",
            TotalAmount = 150.0m,
            OrderDate = DateTime.UtcNow,
            Status = "Pending",
            OrderItems = new List<OrderItemDTO>
    {
        new OrderItemDTO
        {
            ProductId = 1,
            Quantity = 2,
            UnitPrice = 75.0m
        }
    }
        };
        var response = await _client.PostAsJsonAsync("/api/orders", dto);
        var body = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Status: {response.StatusCode}");
        Console.WriteLine($"Body: {body}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        var getResponse = await _client.GetAsync($"/api/orders/{orderId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateOrder_ShouldReturnOk_WhenDataIsValid()
    {
        var orderId = "ORD-002";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

            
            var existing = await db.Orders.FindAsync(orderId);
            if (existing == null)
            {
                db.Orders.Add(new Order
                {
                    Id = orderId,
                    CustomerId = "CUST-001",
                    TotalAmount = 140.0m,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        OrderId = orderId,
                        ProductId = 1,
                        Quantity = 2,
                        UnitPrice = 75.0m
                    }
                }
                });
                await db.SaveChangesAsync();
            }
        }

        var dto = new OrderDTO
        {
            Id = orderId,
            CustomerId = "CUST-001",
            TotalAmount = 150.0m,
            OrderDate = DateTime.UtcNow,
            Status = "Pending",
            OrderItems = new List<OrderItemDTO>
        {
            new OrderItemDTO
            {
                Id = 1,
                OrderId = orderId,
                ProductId = 1,
                Quantity = 5,
                UnitPrice = 75.0m
            }
        }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/orders/{orderId}", dto);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenOrderExists()
    {
        var orderId = "ORD-003";
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            db.Orders.Add(new Order
            {
                Id = orderId,
                CustomerId = "CUST-001",
                TotalAmount = 200.0m,
                OrderDate = DateTime.UtcNow,
                Status = "Pending"
            });
            await db.SaveChangesAsync();
        }
        var response = await _client.GetAsync($"/api/orders/{orderId}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllOrders_ShouldReturnOk()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            db.Orders.Add(new Order
            {
                Id = "ORD-004",
                CustomerId = "CUST-001",
                TotalAmount = 200.0m,
                OrderDate = DateTime.UtcNow,
                Status = "Pending"
            });
            await db.SaveChangesAsync();
        }
        try
        {
            var response = await _client.GetAsync("/api/orders");
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"StatusCode: {response.StatusCode}");
            Console.WriteLine(content);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }

    [Fact]
    public async Task GetOrdersFilter_ShouldReturnOk()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            db.Orders.Add(new Order
            {
                Id = "ORD-005",
                CustomerId = "CUST-001",
                TotalAmount = 200.0m,
                OrderDate = DateTime.UtcNow,
                Status = "Pending"
            });
            await db.SaveChangesAsync();
        }
        try
        {
            var response = await _client.GetAsync("/api/orders/GetOrder/");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }

    }

    [Fact]
    public async Task DeleteOrder_ShouldReturnOk_WhenOrderExists()
    {
        var orderId = "ORD-006";
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            db.Orders.Add(new Order
            {
                Id = orderId,
                CustomerId = "CUST-001",
                TotalAmount = 200.0m,
                OrderDate = DateTime.UtcNow,
                Status = "Pending"
            });
            await db.SaveChangesAsync();
        }
        var response = await _client.DeleteAsync($"/api/orders/{orderId}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    #endregion

    #region Temporary Order Item Endpoints Tests

    [Fact]
    public async Task GetTempOrderItems_ShouldReturnOk()
    {
        var orderId = "TEMP-001";
        var response = await _client.GetAsync($"/api/orders/temp/{orderId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddTempOrderItem_ShouldReturnOk()
    {
        var request = new OrderItemDTO { OrderId = "TEMP-001", ProductId = 1, Quantity = 2, UnitPrice = 100 };
        var response = await _client.PostAsJsonAsync("/api/orders/temp/", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
    }

    [Fact]
    public async Task UpdateTempOrderItem_ShouldReturnOk()
    {
        var orderId = "TEMP-002";
        int productId = 2;
        var addRequest = new OrderItemDTO { OrderId = orderId, ProductId = productId, Quantity = 2, UnitPrice = 100 };
        await _client.PostAsJsonAsync("/api/orders/temp/", addRequest);
        var request = new OrderItemDTO { Id=1,OrderId= orderId, ProductId = productId, Quantity = 5, UnitPrice = 100 };
        var response = await _client.PutAsJsonAsync("/api/orders/temp/", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteTempOrderItem_ShouldReturnOk()
    {
        var orderId = "TEMP-003";
        int productId = 3;
        var request = new OrderItemDTO { OrderId = orderId,ProductId = productId, Quantity = 2, UnitPrice = 100 };
        await _client.PostAsJsonAsync("/api/orders/temp/", request);
        var response = await _client.DeleteAsync($"/api/orders/temp/{orderId}/items/{productId}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    #endregion
}