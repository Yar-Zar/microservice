namespace OrderService.Api.Endpoints;
public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithOpenApi();

        group.MapPost("/", SaveOrderAsync).WithTags("Orders");
        group.MapPut("/{orderId}", UpdateOrderAsync).WithTags("Orders");
        group.MapDelete("/{orderId}", DeleteAsync).WithTags("Orders");
        group.MapGet("/{orderId}", GetByIdAsync).WithTags("Orders");
        group.MapGet("/", GetAllOrderAsync).WithTags("Orders");
        group.MapGet("/GetOrder/", GetOrdersAsync).WithTags("Orders");

        //Temp Order Item
        group.MapGet("/temp/{orderId}", GetTempOrderItems).WithTags("Temporary Order Items");
        group.MapPost("/temp/", AddTempOrderItem).WithTags("Temporary Order Items");
        group.MapPut("/temp/", UpdateTempOrderItem).WithTags("Temporary Order Items");
        group.MapDelete("/temp/{orderId}/items/{productId:int}", DeleteTempOrderItem).WithTags("Temporary Order Items");
        return group;
    }

    #region Order
    private static async Task<IResult> SaveOrderAsync(IOrderService orderService, OrderDTO dto, CancellationToken ct)
    => Results.Ok(await orderService.SaveOrderAsync(dto, ct));

    private static async Task<IResult> UpdateOrderAsync(IOrderService orderService, string orderId, OrderDTO dto, CancellationToken ct)
    => Results.Ok(await orderService.UpdateOrderAsync(orderId, dto, ct));
    private static async Task<IResult> DeleteAsync(IOrderService orderService, string orderId, CancellationToken ct)
    => Results.Ok(await orderService.DeleteAsync(orderId, ct));

    private static async Task<IResult> GetByIdAsync(IOrderService orderService, string orderId, CancellationToken ct)
    => Results.Ok(await orderService.GetByIdAsync(orderId, ct));
    private static async Task<IResult> GetAllOrderAsync(IOrderService orderService, [AsParameters] AppFilter filter, CancellationToken ct)
    => Results.Ok(await orderService.GetAllOrderAsync(filter, ct));
    private static async Task<IResult> GetOrdersAsync(IOrderService orderService, [AsParameters] AppFilter filter, CancellationToken ct)
   => Results.Ok(await orderService.GetOrdersAsync(filter, ct));
    #endregion

    #region OrderItem
    private static async Task<IResult> GetTempOrderItems(IOrderItemTempService tempService, string orderId)
    => Results.Ok(await tempService.GetOrderItemTemp(orderId));
    private static async Task<IResult> AddTempOrderItem(IOrderItemTempService tempService, OrderItemDTO request, CancellationToken ct)
    => Results.Ok(await tempService.AddOrderItemTemp(request, ct));
    private static async Task<IResult> UpdateTempOrderItem(IOrderItemTempService tempService, OrderItemDTO request, CancellationToken ct)
    => Results.Ok(await tempService.UpdateOrderItemTemp(request, ct));
    private static async Task<IResult> DeleteTempOrderItem(IOrderItemTempService tempService, string orderId, int productId)
    => Results.Ok(await tempService.DeleteOrderItemTemp(orderId, productId.ToString()));
    #endregion
}

