using Microsoft.AspNetCore.Mvc;
using Messaging;
using Shared;
using System.Collections.Concurrent;

namespace OrderApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private static readonly ConcurrentDictionary<Guid, OrderDto> _orders = new(); 

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
        _logger.LogInformation($"Received order for customer: {orderDto.CustomerName}");

        var newOrderId = Guid.NewGuid();
        orderDto.OrderId = newOrderId;
        
        _orders.TryAdd(newOrderId, orderDto);

        var orderEvent = new OrderCreatedEvent
        {
            OrderId = newOrderId,
            CustomerName = orderDto.CustomerName,
            Amount = orderDto.TotalAmount,
            CardNumber = orderDto.CardNumber,
            CreatedAt = DateTime.UtcNow
        };

        await _producer.SendMessageAsync(orderEvent, "order.created", "order");
        
        _logger.LogInformation($"Order {newOrderId} successfully received and published as an event.");

        return Accepted(new { 
            OrderId = orderEvent.OrderId, 
            Status = "Order Received - Processing via Message Broker" 
        });
    }

    [HttpGet] 
    public IActionResult GetAllOrders()
    {
        _logger.LogInformation("Retrieving all orders.");
        
        return Ok(_orders.Values.ToList()); 
    }

    [HttpGet("{id}")] 
    public IActionResult GetOrderById(Guid id)
    {
        _logger.LogInformation($"Retrieving order with ID: {id}");

        if (_orders.TryGetValue(id, out var order))
        {
            return Ok(order);
        }
        
        _logger.LogWarning($"Order with ID: {id} not found.");
        return NotFound($"Order with ID {id} not found.");
    }
}