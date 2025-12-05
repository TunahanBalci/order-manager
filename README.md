# Microservices Order Management System

This project is a microservices-based Order Management System built with .NET 8 and RabbitMQ. It consists of three main services:
- **OrderApi**: Receives orders and publishes events.
- **PaymentService**: Processes payments.
- **InventoryService**: Manages stock allocation.

## Prerequisites

Before you begin, ensure you have the following installed:
- [Docker Engine & Docker Compose ](https://docs.docker.com/engine/install)

## Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/TunahanBalci/order-manager.git
```

### 2. Configure Secrets
The application uses a `.env` file to manage sensitive configuration (like RabbitMQ credentials).
There is a .env file as a template. Change the secret values to unique values for security reasons.

## Running the Application

Whole application works on Docker containers. After installing Docker Engine, Docker Compose and configuring secrets, the app is ready to launch: 

```bash
docker compose up -d
```

## Architecture

The app consists of 3 layers: Data Layer (EF Core, PostgreSQL) <-> API Layer (OrderAPI) <-> Messaging Layer (RabbitMQ)
