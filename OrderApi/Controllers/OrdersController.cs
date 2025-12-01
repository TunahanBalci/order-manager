using Microsoft.AspNetCore.Mvc;
using Messaging;
using Shared;

namespace OrderApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMessageProducer _producer;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMessageProducer producer, ILogger<OrdersController> logger)
    {
        _producer = producer;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderDto orderDto)
    {
        _logger.LogInformation($"Received order for {orderDto.CustomerName}");

        var orderEvent = new OrderCreatedEvent
        {
            OrderId = Guid.NewGuid(), 
            CustomerName = orderDto.CustomerName,
            Amount = orderDto.TotalAmount,
            CardNumber = orderDto.CardNumber,
            CreatedAt = DateTime.UtcNow
        };

        // Publish to 'order' exchange with 'order.created' routing key
        await _producer.SendMessageAsync(orderEvent, "order.created", "order");

        return Accepted(new { OrderId = orderEvent.OrderId, Status = "Order Received - Pending Payment" });
    }
}
