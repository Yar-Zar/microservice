namespace PaymentService.Api.Endpoints;
public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments")
            .WithTags("Payments")
            .WithOpenApi();

        group.MapPost("/", SavePaymentAsync);
        group.MapPut("/{paymentId:int}", UpdatePaymentAsync);
        group.MapDelete("/{paymentId:int}", DeleteAsync);
        group.MapGet("/{paymentId:int}", GetByIdAsync);
        group.MapGet("/order/{orderId}", GetByOrderIdAsync);
        group.MapGet("/", GetAllPaymentAsync);

        return group;
    }

    #region Payment
    private static async Task<IResult> SavePaymentAsync(IPaymentService paymentService, PaymentDTO dto, CancellationToken ct)
        => Results.Ok(await paymentService.SavePaymentAsync(dto, ct));

    private static async Task<IResult> UpdatePaymentAsync(IPaymentService paymentService, int paymentId, PaymentDTO dto, CancellationToken ct)
        => Results.Ok(await paymentService.UpdatePaymentAsync(paymentId, dto, ct));

    private static async Task<IResult> DeleteAsync(IPaymentService paymentService, int paymentId, CancellationToken ct)
        => Results.Ok(await paymentService.DeleteAsync(paymentId, ct));

    private static async Task<IResult> GetByIdAsync(IPaymentService paymentService, int paymentId, CancellationToken ct)
        => Results.Ok(await paymentService.GetByIdAsync(paymentId, ct));

    private static async Task<IResult> GetByOrderIdAsync(IPaymentService paymentService, string orderId, CancellationToken ct)
        => Results.Ok(await paymentService.GetByOrderIdAsync(orderId, ct));

    private static async Task<IResult> GetAllPaymentAsync(IPaymentService paymentService, [AsParameters] AppFilter filter, CancellationToken ct)
        => Results.Ok(await paymentService.GetAllPaymentAsync(filter, ct));
    #endregion
}

