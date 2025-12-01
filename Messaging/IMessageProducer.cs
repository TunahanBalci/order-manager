namespace Messaging;

public interface IMessageProducer
{
    Task SendMessageAsync<T>(T message, string queueName, string exchangeName = "");
}
