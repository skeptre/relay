# Relay

**Never lose a webhook:** Relay receives, stores, and reliably delivers webhooks to your app.

Relay is an intermediary that sits between webhook senders (like Stripe or GitHub) and your app. When a webhook arrives, Relay saves it immediately, then forwards it to your app, retrying automatically if your app is down or failing. You can see every event in a dashboard and replay the ones that failed.

## Status

🚧 **Under construction — Phase 0 (Foundations).** Nothing is usable yet.

## Planned Features

- **Receive:** A unique URL per endpoint that accepts any webhook and stores it immediately.
- **Inspect:** A live dashboard showing each request's headers, body, and timing.
- **Deliver:** Forwards events to your app with automatic retries and backoff. Failed events can be replayed.
- **Verify:** Checks Stripe and GitHub signatures on incoming webhooks and signs outgoing ones.
- **Local forwarding:** A CLI that streams live webhooks to your `localhost` while you develop.

## Stack

- **Backend:** C# / ASP.NET Core (.NET 10)
- **Database:** PostgreSQL
- **Frontend:** React + TypeScript
- **CLI and SDK:** Python
- **Infrastructure:** Docker

## Architecture Decisions

See [docs/adr/](docs/adr/) for architecture decision records.

## Local Development

### Prerequisites

- .NET SDK version pinned in `global.json`
- Docker with Docker Compose

### 1. Configure environment variables

Copy the example environment file:

```bash
cp infra/.env.example infra/.env
```

Open `infra/.env` and set `POSTGRES_PASSWORD`.

### 2. Start PostgreSQL

```bash
docker compose -f infra/compose.yaml up -d
```

Check that the database container is healthy:

```bash
docker compose -f infra/compose.yaml ps
```

Wait until PostgreSQL reports `healthy` before continuing.

### 3. Configure the database connection string

The password must match `POSTGRES_PASSWORD` in `infra/.env`.

Run this command from the repository root, replacing `<password>` with your actual password:

```bash
dotnet user-secrets set "ConnectionStrings:Relay" 'Host=localhost;Port=5432;Database=relaydb;Username=relay;Password=<password>' --project services/api/Relay.Api
```

User secrets are stored locally on your machine and are not committed to Git.

### 4. Run the API

```bash
dotnet run --project services/api/Relay.Api
```

### 5. Verify the health endpoint

```bash
curl -i http://localhost:5080/health
```

On Windows PowerShell, use `curl.exe`:

```powershell
curl.exe -i http://localhost:5080/health
```

**Expected response when PostgreSQL is reachable:**

```text
HTTP/1.1 200 OK

Healthy
```

The endpoint returns `503 Unhealthy` if the PostgreSQL health check fails.

## Important: PostgreSQL Data Persistence

The PostgreSQL Docker image applies `POSTGRES_*` initialisation variables only when starting with an empty data volume.

Changing `POSTGRES_PASSWORD` in `infra/.env` after the database has been initialised does **not** automatically update the database password.

You have two options:

1. Update the password inside PostgreSQL.
2. Reset the database volume and initialise it again.

To reset the database:

```bash
docker compose -f infra/compose.yaml down -v
docker compose -f infra/compose.yaml up -d
```

**Warning:** The `-v` flag deletes the Compose-managed database volume and all data stored in it. Only use this when you're comfortable losing the existing local database data.
