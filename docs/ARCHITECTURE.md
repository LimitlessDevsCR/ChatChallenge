# Architecture

## Overview

The solution follows a layered architecture.

Business logic is isolated from infrastructure concerns.

Controllers and SignalR Hubs coordinate requests only.

The Bot is implemented as an independent Worker Service and communicates through RabbitMQ.

---

## Solution Structure

```text
Chat.App
  - MVC
  - SignalR
  - Identity
  |
  v
Chat.Application
  - Services
  - Interfaces
  |
  v
Chat.Domain
  - Entities
  |
  v
Chat.Infrastructure
  - EF Core
  - SQLite
  - RabbitMQ
  - Stock API

Chat.Bot
  |
  v
RabbitMQ
  |
  v
Stock API
  |
  v
RabbitMQ
```

---

## Responsibilities

### Chat.App

- MVC
- Authentication
- SignalR
- Dependency Injection

### Chat.Application

- Business logic
- Use cases
- Interfaces

### Chat.Domain

- Entities

### Chat.Infrastructure

- Persistence
- RabbitMQ
- External APIs

### Chat.Bot

- Consume stock requests
- Call external API
- Publish stock quotes

---

## Message Flow

Normal message

```text
Browser
  |
  v
SignalR Hub
  |
  v
Application Service
  |
  v
Repository
  |
  v
SQLite
  |
  v
SignalR
```

Stock command

```text
Browser
  |
  v
SignalR Hub
  |
  v
Application Service
  |
  v
RabbitMQ
  |
  v
Chat.Bot
  |
  v
Stooq API
  |
  v
RabbitMQ
  |
  v
Chat.App
  |
  v
SignalR
```

---

## Project References

```text
Chat.App
  -> Chat.Application
  -> Chat.Infrastructure

Chat.Application
  -> Chat.Domain

Chat.Infrastructure
  -> Chat.Application
  -> Chat.Domain

Chat.Bot
  -> Chat.Application
  -> Chat.Domain
  -> Chat.Infrastructure
```
