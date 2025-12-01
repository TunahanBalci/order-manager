using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Messaging;

public class RabbitMQProducer : IMessageProducer, IDisposable
{
    private readonly RabbitMQConfiguration _configuration;
    private IConnection? _connection;
    private IChannel? _channel;
    private bool _disposed;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private readonly SemaphoreSlim _channelLock = new(1, 1);

    public RabbitMQProducer(RabbitMQConfiguration configuration)
    {
        _configuration = configuration;
    }

    private async Task<IConnection> GetConnectionAsync()
    {
        if (_connection != null && _connection.IsOpen)
        {
            return _connection;
        }

        await _connectionLock.WaitAsync();
        try
        {
            if (_connection != null && _connection.IsOpen)
            {
                return _connection;
            }

            var factory = new ConnectionFactory
            {
                HostName = _configuration.HostName,
                UserName = _configuration.UserName,
                Password = _configuration.Password
            };
            _connection = await factory.CreateConnectionAsync();
            return _connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task<IChannel> GetChannelAsync()
    {
        if (_channel != null && _channel.IsOpen)
        {
            return _channel;
        }

        await _channelLock.WaitAsync();
        try
        {
            if (_channel != null && _channel.IsOpen)
            {
                return _channel;
            }

            var connection = await GetConnectionAsync();
            _channel = await connection.CreateChannelAsync();
            return _channel;
        }
        finally
        {
            _channelLock.Release();
        }
    }

    public async Task SendMessageAsync<T>(T message, string queueName, string exchangeName = "")
    {
        var channel = await GetChannelAsync();

        if (string.IsNullOrEmpty(exchangeName))
        {
            var args = new Dictionary<string, object>
            {
                { "x-queue-type", "quorum" }
            };
            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: args);
        }

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(exchange: exchangeName, routingKey: queueName, body: body);
        
        Console.WriteLine($"[Producer] Sent message to {queueName}: {json}");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _channel?.CloseAsync().Wait();
            _channel?.Dispose();
            _connection?.CloseAsync().Wait();
            _connection?.Dispose();
        }

        _disposed = true;
    }
}
