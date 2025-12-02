namespace Shared;

public class PaymentProcessedEvent
{
    public Guid OrderId { get; set; }
    public bool IsSuccess { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}
