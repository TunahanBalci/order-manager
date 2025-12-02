using Messaging;
using Shared;
using InventoryService.Services;

namespace InventoryService.Consumers;

public class OrderCreatedConsumer : RabbitMQConsumer<OrderCreatedEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(RabbitMQConfiguration config, IServiceScopeFactory scopeFactory, ILogger<OrderCreatedConsumer> logger)
        : base(config, "allocate_stock", "order", "order.created")
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ProcessMessageAsync(OrderCreatedEvent orderEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Allocating stock for Order {orderEvent.OrderId}");

        using var scope = _scopeFactory.CreateScope();
        var inventoryService = scope.ServiceProvider.GetRequiredService<InventoryService.Services.InventoryService>();

        var success = await inventoryService.AllocateAsync(orderEvent.OrderId, orderEvent.Items);

        if (success)
        {
            _logger.LogInformation($"Stock allocated for Order {orderEvent.OrderId}");
        }
        else
        {
            _logger.LogWarning($"Stock allocation failed for Order {orderEvent.OrderId}");
        }
    }
}
