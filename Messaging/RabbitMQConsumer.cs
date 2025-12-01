using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Messaging;

public abstract class RabbitMQConsumer<T> : BackgroundService
{
    private readonly RabbitMQConfiguration _configuration;
    private readonly string _queueName;
    private readonly string _exchangeName;
    private readonly string _routingKey;
    private IConnection? _connection;
    private IChannel? _channel;

    protected RabbitMQConsumer(RabbitMQConfiguration configuration, string queueName, string exchangeName = "", string routingKey = "")
    {
        _configuration = configuration;
        _queueName = queueName;
        _exchangeName = exchangeName;
        _routingKey = routingKey;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration.HostName,
            UserName = _configuration.UserName,
            Password = _configuration.Password
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(null, stoppingToken);

        // dead queue declaration 
        var dlqName = $"{_queueName}.dlq";
        var dlqArgs = new Dictionary<string, object>
        {
            { "x-queue-type", "quorum" }
        };
        await _channel.QueueDeclareAsync(queue: dlqName, durable: true, exclusive: false, autoDelete: false, arguments: dlqArgs, cancellationToken: stoppingToken);

        // main queue declaration
        var args = new Dictionary<string, object>
        {
            { "x-queue-type", "quorum" },
            { "x-dead-letter-exchange", "" }, 
            { "x-dead-letter-routing-key", dlqName } 
        };
        await _channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: args, cancellationToken: stoppingToken);

        // bind to exchange if specified
        if (!string.IsNullOrEmpty(_exchangeName) && !string.IsNullOrEmpty(_routingKey))
        {
            await _channel.ExchangeDeclareAsync(exchange: _exchangeName, type: ExchangeType.Topic, durable: true, autoDelete: false, arguments: null, cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(queue: _queueName, exchange: _exchangeName, routingKey: _routingKey, cancellationToken: stoppingToken);
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var messageString = Encoding.UTF8.GetString(body);
            
            try
            {
                var message = JsonSerializer.Deserialize<T>(messageString);
                if (message != null)
                {
                    await ProcessMessageAsync(message, stoppingToken);
                    await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                }
                else
                {
                    Console.WriteLine("Error: Deserialized message is null. Sending to DLQ.");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");

                // retry strategy with DLQ
                // check delivery count to prevent infinite loops
                long deliveryCount = 0;
                if (ea.BasicProperties.Headers != null && ea.BasicProperties.Headers.TryGetValue("x-delivery-count", out var countObj))
                {
                    deliveryCount = (long)countObj!;
                }

                //
                // max retries
                //
                const int MaxRetries = 3;

                if (deliveryCount < MaxRetries)
                {
                    // requeue for retry
                    Console.WriteLine($"Retrying message (Attempt {deliveryCount + 1})...");
                    await Task.Delay(1000, stoppingToken); // Add delay to prevent busy loop
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
                }
                else
                {
                    // send to DLQ 
                    Console.WriteLine($"Max retries reached. Sending to DLQ: {dlqName}");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                }
            }
        };

        await _channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
        try 
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    protected abstract Task ProcessMessageAsync(T message, CancellationToken cancellationToken);

    public override void Dispose()
    {
        _channel?.CloseAsync().Wait();
        _connection?.CloseAsync().Wait();
        base.Dispose();
    }
}
