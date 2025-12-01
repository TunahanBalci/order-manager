using Messaging;
using PaymentService;

var builder = Host.CreateApplicationBuilder(args);

// RabbitMQ
// Load .env file
var root = Directory.GetCurrentDirectory();
var dotenv = Path.Combine(root, "..", ".env");
DotNetEnv.Env.Load(dotenv);

// RabbitMQ
var rabbitConfig = new RabbitMQConfiguration
{
    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
    UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
    Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest"
};
builder.Services.AddSingleton(rabbitConfig);
builder.Services.AddSingleton<IMessageProducer, RabbitMQProducer>();

builder.Services.AddHostedService<PaymentService.Consumers.PaymentProcessConsumer>();

var host = builder.Build();
host.Run();
