namespace ProductService.Application.DTOs;
public class ProductDTO
{
    public int? TotalCount { get; set; }
    public int? Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public decimal Price { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public int Stock { get; set; }

    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }

}

