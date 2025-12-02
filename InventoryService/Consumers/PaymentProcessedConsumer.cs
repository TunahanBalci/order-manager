using Messaging;
using Shared;
using InventoryService.Services;

namespace InventoryService.Consumers;

public class PaymentProcessedConsumer : RabbitMQConsumer<PaymentProcessedEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentProcessedConsumer> _logger;

    public PaymentProcessedConsumer(RabbitMQConfiguration config, IServiceScopeFactory scopeFactory, ILogger<PaymentProcessedConsumer> logger)
        : base(config, "inventory_payment_update", "payment", "payment.*")
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ProcessMessageAsync(PaymentProcessedEvent paymentEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Processing payment update for Order {paymentEvent.OrderId}. Success: {paymentEvent.IsSuccess}");

        using var scope = _scopeFactory.CreateScope();
        var inventoryService = scope.ServiceProvider.GetRequiredService<InventoryService.Services.InventoryService>();

        if (paymentEvent.IsSuccess)
        {
            await inventoryService.CommitAsync(paymentEvent.OrderId, paymentEvent.Items);
        }
        else
        {
            await inventoryService.ReleaseAsync(paymentEvent.OrderId, paymentEvent.Items);
        }
    }
}
