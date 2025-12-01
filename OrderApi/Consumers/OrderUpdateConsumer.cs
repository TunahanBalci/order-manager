using Messaging;
using Shared;
using System.Text.Json;

namespace OrderApi.Consumers;

public class OrderUpdateConsumer : RabbitMQConsumer<PaymentProcessedEvent>
{
    private readonly ILogger<OrderUpdateConsumer> _logger;

    public OrderUpdateConsumer(RabbitMQConfiguration config, ILogger<OrderUpdateConsumer> logger) 
        : base(config, "order_updates", "payment", "payment.*")
    {
        _logger = logger;
    }

    protected override async Task ProcessMessageAsync(PaymentProcessedEvent paymentEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Received payment update for Order {paymentEvent.OrderId}: Success={paymentEvent.IsSuccess}, Reason={paymentEvent.Reason}");

        if (paymentEvent.IsSuccess)
        {
            _logger.LogInformation($"Order {paymentEvent.OrderId} CONFIRMED.");
        }
        else
        {
            _logger.LogInformation($"Order {paymentEvent.OrderId} CANCELLED. Reason: {paymentEvent.Reason}");
        }

        await Task.CompletedTask;
    }
}
