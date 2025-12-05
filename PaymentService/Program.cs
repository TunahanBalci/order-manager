using Messaging;
using PaymentService;

var builder = Host.CreateApplicationBuilder(args);

// RabbitMQ
var rabbitConfig = new RabbitMQConfiguration();
builder.Configuration.GetSection("RabbitMQ").Bind(rabbitConfig);
builder.Services.AddSingleton(rabbitConfig);
builder.Services.AddSingleton<IMessageProducer, RabbitMQProducer>();

builder.Services.AddHostedService<PaymentService.Consumers.PaymentProcessConsumer>();

var host = builder.Build();
host.Run();
