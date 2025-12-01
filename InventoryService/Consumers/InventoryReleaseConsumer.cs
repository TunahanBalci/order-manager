using Messaging;
using Shared;

namespace InventoryService.Consumers;

public class InventoryReleaseConsumer : RabbitMQConsumer<PaymentProcessedEvent>
{
    private readonly ILogger<InventoryReleaseConsumer> _logger;

    public InventoryReleaseConsumer(RabbitMQConfiguration config, ILogger<InventoryReleaseConsumer> logger) 
        : base(config, "inventory_release", "payment", "payment.failed")
    {
        _logger = logger;
    }

    protected override async Task ProcessMessageAsync(PaymentProcessedEvent paymentEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"[Inventory] Payment Failed. Releasing stock for Order {paymentEvent.OrderId}.");
        await Task.CompletedTask;
    }
}
