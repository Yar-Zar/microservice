namespace PaymentService.IntegrationTest;
public class PaymentEndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public PaymentEndpointsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        // Reset the database after each test if using Respawn
        await _factory.ResetDatabaseAsync();
    }

    [Fact]
    public async Task SavePayment_And_GetById_ReturnsSuccess()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var dto = new PaymentDTO
        {
            OrderId = "ORD-INT-001",
            Amount = 250.00m,
            Currency = "USD",
            PaymentStatus = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        // Act - Save Payment (POST)
        var postResponse = await _client.PostAsJsonAsync("/api/payments", dto);

        // Assert - Save
        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
      
    }

    [Fact]
    public async Task UpdatePayment_ReturnsSuccess()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var dto = new PaymentDTO
        {
            OrderId = "ORD-INT-001",
            Amount = 250.00m,
            Currency = "USD",
            PaymentStatus = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        // ပထမဦးစွာ Payment တစ်ခု Save လုပ်ခြင်း
        await _client.PostAsJsonAsync("/api/payments", dto);
        var response = await _client.GetAsync($"/api/payments/order/{dto.OrderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetchedPayment = await response.Content.ReadFromJsonAsync<PaymentDTO>();
        // Update DTO ပြင်ဆင်ခြင်း
        dto.Id = fetchedPayment.Id;
        dto.PaymentStatus = "Paid";
        dto.Amount = 300.00m;

        // Act - Update Payment (PUT)
        var updateResponse = await _client.PutAsJsonAsync($"/api/payments/{dto.Id}", dto);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetByOrderId_WhenExists_ReturnsSuccess()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var dto = new PaymentDTO
        {
            OrderId = "ORD-ORDER-999",
            Amount = 150.00m,
            Currency = "USD",
            PaymentStatus = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _client.PostAsJsonAsync("/api/payments", dto);

        // Act - Get By OrderId (GET)
        var response = await _client.GetAsync($"/api/payments/order/{dto.OrderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetchedPayment = await response.Content.ReadFromJsonAsync<PaymentDTO>();
        fetchedPayment.Should().NotBeNull();
        fetchedPayment!.OrderId.Should().Be("ORD-ORDER-999");
    }

    [Fact]
    public async Task GetAllPayments_ReturnsSuccess()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var dto = new PaymentDTO
        {
            OrderId = "ORD-ALL-01",
            Amount = 50.00m,
            Currency = "USD",
            PaymentStatus = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _client.PostAsJsonAsync("/api/payments", dto);

        // Act - Get All (GET) with query filters if needed
        var response = await _client.GetAsync("/api/payments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payments = await response.Content.ReadFromJsonAsync<IEnumerable<PaymentDTO>>();
        payments.Should().NotBeNull();
    }

    [Fact]
    public async Task GetById_WhenPaymentNotFound_ReturnsNotFoundOrError()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        // Act
        var response = await _client.GetAsync("/api/payments/999");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task DeletePayment_WhenExists_ReturnsSuccess()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var dto = new PaymentDTO
        {
            OrderId = "ORD-INT-002",
            Amount = 100.00m,
            Currency = "USD",
            PaymentStatus = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        // အရင်ဆုံး Payment တစ်ခု Save လုပ်ခြင်း
        await _client.PostAsJsonAsync("/api/payments", dto);
        var response = await _client.GetAsync($"/api/payments/order/{dto.OrderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetchedPayment = await response.Content.ReadFromJsonAsync<PaymentDTO>();
        dto.Id = fetchedPayment.Id;
        // Act - Delete Payment (DELETE)
        var deleteResponse = await _client.DeleteAsync($"/api/payments/{dto.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // ထပ်စစ်ဆေးရန် - ၎င်း ID ဖြင့် ထပ်ခေါ်ကြည့်ပါက မရှိတော့ကြောင်း စစ်ဆေးခြင်း
        var getResponse = await _client.GetAsync($"/api/payments/{dto.Id}");
        getResponse.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }
}