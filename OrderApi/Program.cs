using Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
builder.Services.AddHostedService<OrderApi.Consumers.OrderUpdateConsumer>();

var app = builder.Build();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
