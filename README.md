# Relay

Never lose a webhook: Relay receives, stores and reliably delivers webhooks to your app.

Relay is an intermediary that sits between webhook senders (like Stripe or GitHub) and your app. When a webhook arrives, Relay saves it immediately, then forwards it to your app, retrying automatically if your app is down or failing. You can see every event in a dashboard and replay the ones that failed.

## Status

🚧 Under construction: Phase 0 (foundations). Nothing is usable yet.

## Planned features

- **Receive:** a unique URL per endpoint that accepts any webhook and stores it instantly.
- **Inspect:** a live dashboard showing each request's headers, body and timing.
- **Deliver:** forwards events to your app with automatic retries and backoff; failed events can be replayed.
- **Verify:** checks Stripe and GitHub signatures on incoming webhooks and signs outgoing ones.
- **Local forwarding:** a CLI that streams live webhooks to your `localhost` while you develop.

## Stack

C# / ASP.NET Core (.NET 10), PostgreSQL, React + TypeScript, Python (CLI and SDK), Docker.

## Architecture Decisions

[docs/adr/](docs/adr/)

## Local development

Prerequisites: the .NET SDK version pinned in global.json, and Docker.

Create the env file and set a password:
cp infra/.env.example infra/.env, then edit POSTGRES_PASSWORD
Start Postgres and wait for healthy:
docker compose -f infra/compose.yaml up -d
docker compose -f infra/compose.yaml ps
Set the connection string. The password must match infra/.env:
dotnet user-secrets set "ConnectionStrings:Relay" 'Host=localhost;Port=5432;Database=relaydb;Username=relay;Password=<password>' --project services/api/Relay.Api
Run the API:
dotnet run --project services/api/Relay.Api
Check health (on Windows PowerShell, use curl.exe):
curl -i http://localhost:5080/health
Gotcha: the Postgres image applies POSTGRES_* only on first start with an empty volume. If you change the password later, either update it inside the database or reset with docker compose -f infra/compose.yaml down -v. -v deletes all data.
