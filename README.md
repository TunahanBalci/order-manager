# Microservices Order Management System

This project is a microservices-based Order Management System built with .NET 8 and RabbitMQ. It consists of three main services:
- **OrderApi**: Receives orders and publishes events.
- **PaymentService**: Processes payments.
- **InventoryService**: Manages stock allocation.

## Prerequisites

Before you begin, ensure you have the following installed:
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (or Docker Engine + Compose)

## Getting Started

### 1. Clone the Repository
```bash
git clone <repository-url>
cd seng315
```

### 2. Configure Secrets
The application uses a `.env` file to manage sensitive configuration (like RabbitMQ credentials).
Copy the example configuration below to a new file named `.env` in the root directory:

```bash
# Create .env file
touch .env
```

Add the following content to `.env`:
```env
RABBITMQ_HOST=localhost
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
```
> **Note**: You can change the username and password, but ensure you update them here. The `docker-compose.yml` will use these values to initialize RabbitMQ.

## Running the Application

### 1. Start Infrastructure
Start the RabbitMQ container using Docker Compose. This will use the credentials defined in your `.env` file.

```bash
docker compose up -d
```

### 2. Start Microservices
You need to run each service. You can do this by opening separate terminal windows for each service or running them in the background.

**Terminal 1: InventoryService**
```bash
dotnet run --project InventoryService/InventoryService.csproj --urls "http://localhost:5001"
```

**Terminal 2: PaymentService**
```bash
dotnet run --project PaymentService/PaymentService.csproj
```

**Terminal 3: OrderApi**
```bash
dotnet run --project OrderApi/OrderApi.csproj --urls "http://localhost:5046"
```

## Testing

A test script is provided to simulate order flows and verify the system.

```bash
# Ensure the script is executable
chmod +x test_scenarios.sh

# Run the test scenarios
./test_scenarios.sh
```

This script will:
1.  Send a successful order (Amount < 1000).
2.  Send a failed order (Amount >= 1000).
3.  Print the expected outcomes for each service.

## Architecture

- **Messaging**: RabbitMQ is used for asynchronous communication between services.
- **Configuration**: `DotNetEnv` is used to load environment variables from the `.env` file.
