# .NET Chat Challenge

Technical challenge implementing a browser-based chat application using .NET.

## Technology Stack

- .NET 10
- ASP.NET Core MVC
- ASP.NET Core Identity
- SignalR
- Entity Framework Core
- SQLite
- RabbitMQ
- xUnit

## Target Features

- User registration and authentication
- Real-time chat using SignalR
- Stock quote command (`/stock=...`)
- Decoupled Bot using RabbitMQ
- SQLite persistence
- Unit tests

These are the intended final features for the challenge implementation. Until the
application is complete, this list should be treated as the target scope rather
than a verified implementation status.

## Challenge Specification

The original assignment is available at `docs/spec/net-challenge-financial-chat.pdf`.

## Local RabbitMQ

RabbitMQ is required for the stock quote bot flow.

Prerequisite: Docker Desktop must be installed and running.

Start RabbitMQ with Docker Compose:

```powershell
docker compose up -d
```

RabbitMQ will be available at:

- Broker: `localhost:5672`
- Management UI: `http://localhost:15672`
- Username: `guest`
- Password: `guest`

Stop RabbitMQ with:

```powershell
docker compose down
```

## Solution Structure

Chat.App
Chat.Application
Chat.Domain
Chat.Infrastructure
Chat.Bot
Chat.Tests

## Intended Run Flow

1. Start RabbitMQ with `docker compose up -d`.
2. Run Chat.Bot.
3. Run Chat.App.
4. Open two browser windows.
5. Register two users.
6. Start chatting.
