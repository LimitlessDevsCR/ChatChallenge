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

## Solution Structure

Chat.App
Chat.Application
Chat.Domain
Chat.Infrastructure
Chat.Bot
Chat.Tests

## Intended Run Flow

1. Start RabbitMQ.
2. Run Chat.Bot.
3. Run Chat.App.
4. Open two browser windows.
5. Register two users.
6. Start chatting.
