# .NET Chat Challenge

Technical challenge implementing a browser-based chat application using .NET.

## Repository

The repository is public so the reviewers can access it directly:

```text
https://github.com/LimitlessDevsCR/ChatChallenge
```

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

The installer bonus was not implemented. The solution is composed of a web
application, a worker service, SQLite persistence, and RabbitMQ. For this
challenge, Docker Compose for RabbitMQ plus documented .NET commands provides a
clearer and more reproducible setup than a custom installer. Adding an installer
would add packaging complexity outside the main backend scope of the challenge.

## Challenge Specification

The original assignment is available at `docs/spec/net-challenge-financial-chat.pdf`.

## Prerequisites

- .NET 10 SDK
- Docker Desktop
- Git

If Entity Framework tooling is not installed locally, install it with:

```powershell
dotnet tool install --global dotnet-ef
```

## Setup

Restore and build the solution:

```powershell
dotnet restore ChatChallenge.slnx
dotnet build ChatChallenge.slnx
```

Create or update the local SQLite database:

```powershell
dotnet ef database update --project Chat.Infrastructure --startup-project Chat.App
```

The default local connection string is:

```json
"DefaultConnection": "Data Source=chat.db"
```

This creates `Chat.App/chat.db` locally. The database file is intentionally not
committed.

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

## Run Locally

Start RabbitMQ first:

```powershell
docker compose up -d
```

Run the bot worker in one terminal:

```powershell
dotnet run --project Chat.Bot
```

Run the web application in another terminal:

```powershell
dotnet run --project Chat.App
```

Open the app at:

```text
https://localhost:7098
```

If the HTTPS profile is not available, use:

```text
http://localhost:5006
```

Register two users in separate browser windows or one normal window and one
private/incognito window. Open the Chat page, join a room, and send messages.

To test the stock command, send:

```text
/stock=aapl.us
```

The user command is not persisted as a chat message. The bot response is
persisted and shown as a message from `Stock Bot`.

## Tests

Run the unit tests with:

```powershell
dotnet test Chat.Tests/Chat.Tests.csproj
```

## Security Notes

- Local SQLite database files are ignored by Git (`*.db`, `*.db-shm`, `*.db-wal`).
- Visual Studio and build artifacts are ignored (`.vs`, `bin`, `obj`, `*.user`).
- `appsettings.Development.json` is committed only with local development defaults and does not contain secrets.
- RabbitMQ uses the default `guest` / `guest` credentials only for the local Docker Compose setup.
- No API keys, access tokens, or production secrets are required for this challenge.

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
page to server-side clients such as `curl` or `HttpClient`.

The stock quote provider behavior is handled based on the observed service
response. When Stooq returns valid CSV, the bot parses it and sends the quote
back to the chat. When Stooq returns `404 Not Found` or HTML instead of CSV, the
bot handles the failure and posts that the quote is not available without
breaking the chat flow.

The CSV parser is implemented and covered by unit tests for both the challenge
CSV shape and the historical CSV shape.

## Solution Structure

```text
Chat.App            ASP.NET Core MVC, Identity UI, SignalR Hub
Chat.Application    Business services, DTOs, interfaces
Chat.Domain         Domain entities
Chat.Infrastructure EF Core, SQLite, RabbitMQ, Stooq client
Chat.Bot            Worker Service that processes stock quote requests
Chat.Tests          Unit tests
```

## Intended Run Flow

1. Start RabbitMQ with `docker compose up -d`.
2. Run Chat.Bot.
3. Run Chat.App.
4. Open two browser windows.
5. Register two users.
6. Start chatting.
