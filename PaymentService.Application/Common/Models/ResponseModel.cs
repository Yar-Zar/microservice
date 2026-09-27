namespace PaymentService.Application.Common.Models;
public class ResponseModel
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public string? StatusCode { get; set; }

}
