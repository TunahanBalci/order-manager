using Messaging;
using InventoryService.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// RabbitMQ Configuration
// Load .env file
var root = Directory.GetCurrentDirectory();
var dotenv = Path.Combine(root, "..", ".env");
DotNetEnv.Env.Load(dotenv);

// RabbitMQ Configuration
var rabbitConfig = new RabbitMQConfiguration
{
    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
    UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
    Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest"
};
builder.Services.AddSingleton(rabbitConfig);

// Register Consumers
builder.Services.AddHostedService<InventoryAllocateConsumer>();
builder.Services.AddHostedService<InventoryCommitConsumer>();
builder.Services.AddHostedService<InventoryReleaseConsumer>();

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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
