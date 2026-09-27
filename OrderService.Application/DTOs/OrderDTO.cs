using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Application.DTOs;
public class OrderDTO
{
    public int? TotalCount { get; set; }

    [Column(TypeName = "varchar(20)")]
    public string? Id { get; set; }

    [Required]
    [Column(TypeName = "varchar(15)")]
    public string CustomerId { get; set; }

    [Required]
    public DateTime OrderDate { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)]")]
    public decimal TotalAmount { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    public string Status { get; set; }
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "varchar(50)")]
    public string? CreatedBy { get; set; }

    // Navigation Property
    public ICollection<OrderItemDTO> OrderItems { get; set; } = new List<OrderItemDTO>();

    

}

