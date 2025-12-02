using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OrderApi.Models
{
   public class Order
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string ShippingAddress { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public decimal TotalAmount { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

}