# AGENTS.md

## Project Goal

Implement the .NET Chat Challenge following the provided specification.

## Read First

Before making architectural decisions, review:

- README.md
- docs/spec/net-challenge-financial-chat.pdf
- docs/ARCHITECTURE.md
- docs/IMPLEMENTATION_PLAN.md

## Rules

- Preserve the existing architecture.
- Do not rename projects.
- Keep the frontend intentionally simple.
- Use ASP.NET Core Identity.
- Use Entity Framework Core with SQLite.
- Use SignalR.
- Use RabbitMQ between Chat.App and Chat.Bot.
- Do not place business logic inside Controllers or SignalR Hubs.
- Prefer modifying existing files over creating new ones.
- Avoid over-engineering.
- If requirements are ambiguous, ask before implementing.
