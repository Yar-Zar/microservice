using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Domain.Entities
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

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
}
