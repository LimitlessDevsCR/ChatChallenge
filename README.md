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

## Completed Features

- User registration and authentication
- Real-time chat using SignalR
- Stock quote command (`/stock=...`)
- Decoupled Bot using RabbitMQ
- SQLite persistence
- Unit tests

## Completed Bonus Features

- Multiple chatrooms
- ASP.NET Core Identity authentication
- Bot exception handling

The installer bonus was not implemented.

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

## Stock Quote Endpoint

The challenge specification points to Stooq's quote CSV endpoint:

```text
https://stooq.com/q/l/?s=aapl.us&f=sd2t2ohlcv&h&e=csv
```

During implementation, that endpoint returned `404 Not Found`, so the stock bot
uses Stooq's historical CSV download endpoint instead:

```text
https://stooq.com/q/d/l/?s=aapl.us&i=d
```

This endpoint returns CSV in a browser and provides the latest available close
price from the historical data. Stooq may still return a JavaScript verification
page to server-side clients such as `curl` or `HttpClient`; when that happens,
the bot handles the failure and posts that the quote is not available.

The CSV parser is implemented and covered by unit tests for both the challenge
CSV shape and the historical CSV shape. The remaining limitation is external to
the application: Stooq may refuse to return CSV to non-browser clients.

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
