using Messaging;
using Shared;

namespace InventoryService.Consumers;

public class InventoryCommitConsumer : RabbitMQConsumer<PaymentProcessedEvent>
{
    private readonly ILogger<InventoryCommitConsumer> _logger;

    public InventoryCommitConsumer(RabbitMQConfiguration config, ILogger<InventoryCommitConsumer> logger) 
        : base(config, "inventory_commit", "payment", "payment.success")
    {
        _logger = logger;
    }

    protected override async Task ProcessMessageAsync(PaymentProcessedEvent paymentEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"[Inventory] Payment Success. Committing stock for Order {paymentEvent.OrderId}.");
        await Task.CompletedTask;
    }
}
