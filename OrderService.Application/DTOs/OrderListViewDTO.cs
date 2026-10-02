using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.DTOs;

public class OrderListViewDTO
{
    public string OrderId { get; set; }
    public string OrderStatus { get; set; }

    // Payment Information (Nullable in case payment is not found)
    public int? PaymentId { get; set; }
    public decimal? Amount { get; set; }
    public string Currency { get; set; }
    public string PaymentStatus { get; set; }

    // Order Items with Product details
    public List<OrderItemDetailDTO> Items { get; set; } = new();
}
public class OrderItemDetailDTO
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
