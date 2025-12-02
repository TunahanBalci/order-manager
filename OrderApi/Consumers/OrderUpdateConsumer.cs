using Messaging;
using Shared;
using System.Text.Json;

namespace OrderApi.Consumers;

public class OrderUpdateConsumer : RabbitMQConsumer<PaymentProcessedEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderUpdateConsumer> _logger;

    public OrderUpdateConsumer(RabbitMQConfiguration config, IServiceScopeFactory scopeFactory, ILogger<OrderUpdateConsumer> logger) 
        : base(config, "order_updates", "payment", "payment.*")
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ProcessMessageAsync(PaymentProcessedEvent paymentEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Received payment update for Order {paymentEvent.OrderId}: Success={paymentEvent.IsSuccess}, Reason={paymentEvent.Reason}");

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderApi.Data.OrderDbContext>();

        var order = await context.Orders.FindAsync(paymentEvent.OrderId);
        if (order != null)
        {
            if (paymentEvent.IsSuccess)
            {
                order.Status = OrderApi.Models.OrderStatus.Completed;
                _logger.LogInformation($"Order {paymentEvent.OrderId} CONFIRMED.");
            }
            else
            {
                order.Status = OrderApi.Models.OrderStatus.Failed;
                _logger.LogInformation($"Order {paymentEvent.OrderId} CANCELLED. Reason: {paymentEvent.Reason}");
            }
            await context.SaveChangesAsync();
        }
        else
        {
             _logger.LogWarning($"Order {paymentEvent.OrderId} not found for update.");
        }
    }
}
