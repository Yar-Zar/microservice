namespace OrderService.IntegrationTests;
public static class GrpcMockHelper
{
    // 1. Payment gRPC Client Mock Helper
    public static void SetupGetPaymentsByOrderIds(
        this Mock<PaymentGrpcServiceClient> mockClient,
        IEnumerable<PaymentResponseDto>? customItems = null)
    {
        var response = new PaymentListResponse();

        if (customItems != null)
        {
            response.Items.AddRange(customItems);
        }
        else
        {
            // Default Sample Data
            response.Items.Add(new PaymentResponseDto
            {
                Id = 1,
                OrderId = "ORD-001",
                Amount = 150.0,
                Currency = "USD",
                PaymentStatus = "Completed"
            });
        }

        mockClient
            .Setup(client => client.GetPaymentsByOrderIdsAsync(
                It.IsAny<RequestOrderIdList>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new AsyncUnaryCall<PaymentListResponse>(
                Task.FromResult(response),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { }));
    }

    // 2. Product gRPC Client Mock Helper
    public static void SetupGetProductsByIds(
        this Mock<ProductGrpcServiceClient> mockClient,
        IEnumerable<ProductResponseDto>? customItems = null)
    {
        var response = new ProductListResponse();

        if (customItems != null)
        {
            response.Items.AddRange(customItems);
        }
        else
        {
            // Default Sample Data
            response.Items.Add(new ProductResponseDto
            {
                Id = 1,
                Name = "Sample Product",
                Price = 75.0
            });
        }

        mockClient
            .Setup(client => client.GetProductsByIdsAsync(
                It.IsAny<RequestProductIds>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new AsyncUnaryCall<ProductListResponse>(
                Task.FromResult(response),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { }));
    }
}
