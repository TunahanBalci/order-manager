using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OrderApi.Models
{
    public class OrderItem
    {
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string ProductName { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;


    }
}