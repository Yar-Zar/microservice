namespace ProductService.IntegrationTests;

public class ProductEndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ProductEndpointsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {

        await _factory.ResetDatabaseAsync();
    }

    [Fact]
    public async Task CreateProduct_And_GetProducts_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateProductCommand(
            new ProductDTO
            {
                Name = "Integration Test Product",
                Price = 250.00m,
                Stock = 10,
                Description = "Integration Test Description"
            }
        );

        // Act - Create Product
        var createResponse = await _client.PostAsJsonAsync("/api/products", command);

        // Assert Create
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseModel = await createResponse.Content.ReadFromJsonAsync<ResponseModel>();
        responseModel.Should().NotBeNull();
        responseModel!.IsSuccess.Should().BeTrue();

        // Act - Get Products List
        var getResponse = await _client.GetAsync("/api/products");

        // Assert Get List
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await getResponse.Content.ReadFromJsonAsync<IEnumerable<ProductDTO>>();
        products.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateProduct_ReturnsSuccess()
    {
        // Arrange - Create initial product
        var createCommand = new CreateProductCommand(
            new ProductDTO
            {
                Name = "Product For Update",
                Price = 100.00m,
                Stock = 5,
                Description = "Old Description"
            }
        );
        await _client.PostAsJsonAsync("/api/products", createCommand);

        // Fetch products to get the assigned Id
        var getResponse = await _client.GetAsync("/api/products");
        var products = await getResponse.Content.ReadFromJsonAsync<List<ProductDTO>>();
        var targetProduct = products!.First(p => p.Name == "Product For Update");
        targetProduct.Should().NotBeNull();
        var updatedDto = new ProductDTO
        {
            Id = targetProduct.Id,
            Name = "Updated Product Name",
            Price = 150.00m,
            Stock = 20,
            Description = "Updated Description"
        };

        // Act - Update Product Endpoint (`PUT /api/products/{id}`)
        var updateResponse = await _client.PutAsJsonAsync($"/api/products/{targetProduct.Id}", updatedDto);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await updateResponse.Content.ReadFromJsonAsync<ResponseModel>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
    }
    [Fact]
    public async Task GetProducts_WithFilter_ReturnsSuccess()
    {
        // Arrange - Seed a product first
        var createCommand = new CreateProductCommand(
            new ProductDTO
            {
                Name = "Filterable Product",
                Price = 300.00m,
                Stock = 12,
                Description = "Search and Filter Test"
            }
        );
        await _client.PostAsJsonAsync("/api/products", createCommand);

        // Act - Call GetProducts endpoint with query parameters (AppFilter)
        var response = await _client.GetAsync("/api/products?CurrentPageNo=1&CurrentRowLimit=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<IEnumerable<ProductDTO>>();
        products.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductsForDropdown_ReturnsSuccess()
    {
        // Arrange - Seed a product first so dropdown has data
        var createCommand = new CreateProductCommand(
            new ProductDTO
            {
                Name = "Dropdown Product",
                Price = 50.00m,
                Stock = 15
            }
        );
        await _client.PostAsJsonAsync("/api/products", createCommand);

        // Act
        var response = await _client.GetAsync("/api/products/dropdown");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dropdowns = await response.Content.ReadFromJsonAsync<IEnumerable<DropdownDto>>();
        dropdowns.Should().NotBeNull();
        dropdowns.Should().NotBeEmpty();
    }
}