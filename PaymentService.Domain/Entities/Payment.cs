using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Domain.Entities
{
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
        public string PaymentStatus { get; set; } // Pending, Completed, Failed

        
        [Column(TypeName = "varchar(100)")]
        public string? TransactionId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
