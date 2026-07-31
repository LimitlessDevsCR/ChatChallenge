# Implementation Plan

## Phase 1 - Completed

- Solution structure
- Project references
- EF Core
- SQLite
- Identity persistence
- Initial migration
- Local database creation

## Phase 2 - Completed

- MVC
- Authentication
- SignalR
- Authenticated chat page
- Chat rooms

## Phase 3 - Completed

- Message persistence
- Last 50 messages
- Room-specific message history

## Phase 4 - Completed

- RabbitMQ
- Worker Service
- Stock request queue
- App-to-Bot stock command publishing
- Bot-to-App stock response queue
- Bot response persistence and SignalR broadcast

## Phase 5 - Completed with External Provider Limitation

- Stock API
- CSV parsing
- Stooq client
- Bot error response handling
- Parser coverage for challenge CSV format
- Parser coverage for historical Stooq CSV format

Known limitation: Stooq's challenge endpoint returned `404 Not Found` during
implementation. The historical CSV endpoint returns CSV in a browser, but may
return a JavaScript verification page to server-side clients such as `HttpClient`.
When Stooq does not return valid CSV, the bot handles the failure and sends an
unavailable quote response instead of breaking the chat flow.

## Phase 6 - Completed

- Unit tests

## Phase 7 - Completed

- Documentation

## Optional Bonus Not Implemented

- Installer
