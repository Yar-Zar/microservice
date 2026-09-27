using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.DTOs
{
    public class OrderItemDTO
    {
        public int? Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
    }
}
