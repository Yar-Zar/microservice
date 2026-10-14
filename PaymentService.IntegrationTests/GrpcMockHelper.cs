namespace PaymentService.IntegrationTests;
public static class GrpcMockHelper
{
    
    public static void SetupUpdateOrderStatus(
        this Mock<OrderGrpcService.OrderGrpcServiceClient> mockClient,
        bool success = true)
    {
        var response = new UpdateOrderStatusResponse { Success = success };
        mockClient
             .Setup(client => client.UpdateOrderStatusAsync(
                 It.IsAny<UpdateOrderStatusRequest>(),
                 It.IsAny<Metadata>(),
                 It.IsAny<DateTime?>(),
                 It.IsAny<CancellationToken>()))
             .Returns(new AsyncUnaryCall<UpdateOrderStatusResponse>(
                 Task.FromResult(response),
                 Task.FromResult(new Metadata()),
                 () => Status.DefaultSuccess,
                 () => new Metadata(),
                 () => { }));
    }
}
