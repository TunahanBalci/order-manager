using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Messaging;
using Shared;
using OrderApi.Data;
using OrderApi.Models;

namespace OrderApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderDbContext _context;
    private readonly IMessageProducer _producer;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        OrderDbContext context,
        IMessageProducer producer,
        ILogger<OrdersController> logger)
    {
        _context = context;
        _producer = producer;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderDto orderDto)
    {
        _logger.LogInformation("Received order for customer: {Customer}", orderDto.CustomerName);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = orderDto.CustomerName,
            ShippingAddress = orderDto.ShippingAddress,
            Status = OrderStatus.Pending,
            TotalAmount = orderDto.TotalAmount,
            Items = orderDto.Items?.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList() ?? new List<OrderItem>()
        };

        orderDto.OrderId = order.Id;

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var orderEvent = new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerName = order.CustomerName,
            Amount = order.TotalAmount,
            CardNumber = orderDto.CardNumber,
            CreatedAt = DateTime.UtcNow,
            Items = orderDto.Items ?? new List<OrderItemDto>()
        };

        await _producer.SendMessageAsync(orderEvent, "order.created", "order");

        _logger.LogInformation("Order {OrderId} saved to DB and published as event.", order.Id);

        return Accepted(new
        {
            OrderId = order.Id,
            Status = "Order stored in database and processing via message broker"
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        _logger.LogInformation("Retrieving all orders from database.");

        var orders = await _context.Orders
            .Include(o => o.Items)
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        _logger.LogInformation("Retrieving order with ID: {OrderId}", id);

        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
        {
            _logger.LogWarning("Order with ID {OrderId} not found in database.", id);
            return NotFound($"Order with ID {id} not found.");
        }

        return Ok(order);
    }
}