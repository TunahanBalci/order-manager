namespace Shared;

public class OrderDto
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string CardNumber { get; set; } = string.Empty; // Simplified for demo
}
