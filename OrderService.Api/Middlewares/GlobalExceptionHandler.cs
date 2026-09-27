namespace OrderService.Api.Middlewares;
public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.TraceIdentifier;
            _logger.LogError(ex,
            "Unhandled exception at {Path} with CorrelationId {CorrelationId}. Message: {Message}",
            context.Request.Path,
            correlationId,
            ex.Message);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,       // 401
            FluentValidation.ValidationException => HttpStatusCode.BadRequest,// 400
            InvalidOperationException => HttpStatusCode.BadRequest,             // 400
            SecurityException => HttpStatusCode.Forbidden,                    // 403
            KeyNotFoundException => HttpStatusCode.NotFound,                  // 404
            DuplicateException => HttpStatusCode.Conflict,             // 409
            ArgumentNullException => HttpStatusCode.BadRequest,               // 400
            ArgumentException => HttpStatusCode.BadRequest,                   // 400
            TimeoutException => HttpStatusCode.RequestTimeout,                // 408
            DbUpdateException => HttpStatusCode.InternalServerError,          // 500
            OperationCanceledException => (HttpStatusCode)499,                // 499 (Client Closed Request)
            _ => HttpStatusCode.InternalServerError                          // 500
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ResponseModel
        {
            IsSuccess = false,
            
            Message = statusCode == HttpStatusCode.InternalServerError
                      ? "An unexpected error occurred. Please try again later."
                      : exception.Message,
            StatusCode = ((int)statusCode).ToString()
        };

        var json = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(json);
    }
}

