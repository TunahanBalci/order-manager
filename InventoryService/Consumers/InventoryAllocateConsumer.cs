using Messaging;
using Shared;

namespace InventoryService.Consumers;

public class InventoryAllocateConsumer : RabbitMQConsumer<OrderCreatedEvent>
{
    private readonly ILogger<InventoryAllocateConsumer> _logger;

    public InventoryAllocateConsumer(RabbitMQConfiguration config, ILogger<InventoryAllocateConsumer> logger) 
        : base(config, "inventory_allocate", "order", "order.created")
    {
        _logger = logger;
    }

    protected override async Task ProcessMessageAsync(OrderCreatedEvent orderEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"[Inventory] Allocating stock for Order {orderEvent.OrderId}...");
        await Task.Delay(100, cancellationToken); // Simulate DB work
        _logger.LogInformation($"[Inventory] Stock allocated for Order {orderEvent.OrderId}.");
    }
}
