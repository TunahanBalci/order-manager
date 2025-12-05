using InventoryService.Data;
using InventoryService.Services;
using Messaging;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Data Layer (InMemory for simplicity in this homework)
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseInMemoryDatabase("InventoryDb"));

// Domain Services
builder.Services.AddScoped<InventoryService.Services.InventoryService>();

// RabbitMQ
var rabbitConfig = new RabbitMQConfiguration();
builder.Configuration.GetSection("RabbitMQ").Bind(rabbitConfig);
builder.Services.AddSingleton(rabbitConfig);
builder.Services.AddSingleton<IMessageProducer, RabbitMQProducer>();

// Consumers
builder.Services.AddHostedService<InventoryService.Consumers.OrderCreatedConsumer>();
builder.Services.AddHostedService<InventoryService.Consumers.PaymentProcessedConsumer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
