# Messaging Layer Integration Guide

This guide explains how to integrate the `Messaging` library into your ASP.NET Core applications (e.g., OrderApi, PaymentService).

## 1. Setup

### Add Reference
Ensure your project references the `Messaging` project.
```xml
<ProjectReference Include="..\Messaging\Messaging.csproj" />
```

### Configuration
Add RabbitMQ settings to your `appsettings.json` or environment variables:
```json
"RabbitMQ": {
  "HostName": "localhost",
  "UserName": "guest",
  "Password": "guest"
}
```

### Dependency Injection
In your `Program.cs`, register the RabbitMQ configuration and producer:

```csharp
using Messaging;

// ...

// 1. Configure RabbitMQ Settings
var rabbitConfig = new RabbitMQConfiguration
{
    HostName = "localhost", // Or read from config
    UserName = "guest",
    Password = "guest"
};
builder.Services.AddSingleton(rabbitConfig);

// 2. Register Producer
builder.Services.AddSingleton<IMessageProducer, RabbitMQProducer>();
```

## 2. Producing Messages

Inject `IMessageProducer` into your controllers or services.

```csharp
public class OrderController : ControllerBase
{
    private readonly IMessageProducer _producer;

    public OrderController(IMessageProducer producer)
    {
        _producer = producer;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(OrderDto order)
    {
        // ... business logic ...

        // Send message to "order-created" queue
        await _producer.SendMessageAsync(order, "order-created");

        return Ok();
    }
}
```

## 3. Consuming Messages

To consume messages, create a class that inherits from `RabbitMQConsumer`.

```csharp
using Messaging;

public class OrderCreatedConsumer : RabbitMQConsumer<OrderDto>
{
    public OrderCreatedConsumer(RabbitMQConfiguration config) 
        : base(config, "order-created") // Specify queue name here
    {
    }

    protected override async Task ProcessMessageAsync(OrderDto order, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Processing order: {order.Id}");
        
        // Simulate work
        await Task.Delay(100, cancellationToken);
    }
}
```

### Register Consumer
Register your consumer as a Hosted Service in `Program.cs`:

```csharp
builder.Services.AddHostedService<OrderCreatedConsumer>();
```

## 4. Key Features & Best Practices

- **Persistent Connection**: The producer maintains a single connection for performance.
- **Thread Safety**: Connection creation is thread-safe.
- **Reliability**: Consumers automatically requeue messages if processing fails (transient error handling).
- **Quorum Queues**: Queues are declared as Quorum queues for high availability.
