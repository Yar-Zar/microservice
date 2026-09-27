namespace PaymentService.Application.DTOs;
public class PaymentDTO
{
    public int? TotalCount { get; set; }

    public int Id { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    public string OrderId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; }

    [Required]
    [MaxLength(50)]
    public string PaymentStatus { get; set; } 


    [Column(TypeName = "varchar(100)")]
    public string? TransactionId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "varchar(50)")]
    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

}

