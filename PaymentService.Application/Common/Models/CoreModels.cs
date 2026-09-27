namespace PaymentService.Application.Common.Models;
public record DropdownDto(int Id, string Text);
public record PaymentCompletedEvent(string OrderId);
public class AppFilter
{
    public int? CurrentPageNo { get; set; }
    public int? CurrentRowLimit { get; set; }
    public string? SearchInput { get; set; }
    public string? SortColumn { get; set; }
    public string? SortDirection { get; set; }
    public bool? IsPageSize { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

