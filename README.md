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

### Architecture Decisions

[docs/adr/](docs/adr/)
