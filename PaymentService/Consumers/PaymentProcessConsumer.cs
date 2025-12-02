using Messaging;
using Shared;

namespace PaymentService.Consumers;

public class PaymentProcessConsumer : RabbitMQConsumer<OrderCreatedEvent>
{
    private readonly IMessageProducer _producer;
    private readonly ILogger<PaymentProcessConsumer> _logger;

    public PaymentProcessConsumer(RabbitMQConfiguration config, IMessageProducer producer, ILogger<PaymentProcessConsumer> logger) 
        : base(config, "process_payment", "order", "order.created")
    {
        _producer = producer;
        _logger = logger;
    }

    protected override async Task ProcessMessageAsync(OrderCreatedEvent orderEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Processing payment for Order {orderEvent.OrderId} (Amount: {orderEvent.Amount})");

        // simulate payment processing
        await Task.Delay(500, cancellationToken);

        var isSuccess = orderEvent.Amount < 1000; // fail if amount >= 1000
        var paymentEvent = new PaymentProcessedEvent
        {
            OrderId = orderEvent.OrderId,
            IsSuccess = isSuccess,
            Reason = isSuccess ? "Authorized" : "Insufficient Funds",
            Timestamp = DateTime.UtcNow,
            Items = orderEvent.Items
        };

        var routingKey = isSuccess ? "payment.success" : "payment.failed";
        await _producer.SendMessageAsync(paymentEvent, routingKey, "payment");
        
        _logger.LogInformation($"Payment processed: {routingKey}");
    }
}
