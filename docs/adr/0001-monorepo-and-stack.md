# ADR-0001: One monorepo, with C#/.NET for the core and Postgres for storage

- **Status:** Accepted
- **Date:** 2026-11-17

## Context

Relay has several parts that ship together: an ASP.NET Core API that also runs the delivery workers, a React dashboard, a TypeScript client generated from the API's OpenAPI spec, and Python tools (`relay-cli`, a signature-verification SDK, Locust load tests).

These parts share contracts. The dashboard and CLI depend on the API's shape, and the SDK has to match the signing scheme the API uses. A change to one often needs a matching change in another.

The project is built by one person on two machines, macOS and Windows. Anything that only works on one of them is a bug. It is also a portfolio project, so a reviewer should be able to clone one thing and see everything.

## Options considered

### Repository layout

1. **Monorepo:** all parts in one repository, one folder per part (`services/`, `apps/`, `packages/`, `python/`, `infra/`).
   - Pro: a contract change and every consumer change land in one PR and one CI run, and there's one place to clone and review.
   - Con: CI has to avoid rebuilding everything on every change, and tool configs for three languages sit side by side.
2. **Polyrepo:** one repository per part.
   - Pro: each part has its own history, CI and release cycle.
   - Con: cross-repo changes need coordinated PRs and versioned contracts. For one developer that is pure overhead.

### Core language

1. **C# / ASP.NET Core (.NET 10 LTS):** typed, fast, and strong for background workers (`BackgroundService`, `IHttpClientFactory`, `CancellationToken`). EF Core covers normal data access, with raw SQL where the queue needs it. Supported until November 2028.
2. **Go:** good fit for network services, but a second new ecosystem to learn alongside the domain.
3. **Node/TypeScript:** one language with the dashboard, but a weaker fit for CPU-bound and long-running worker code, and weaker types at runtime.

### Database

1. **PostgreSQL:** relational data, `jsonb` for raw headers, `bytea` for bodies, and `SELECT … FOR UPDATE SKIP LOCKED`, which lets it also serve as the job queue for the MVP (decided separately in ADR-0004).
2. **PostgreSQL plus a dedicated broker (SQS, RabbitMQ, Kafka):** a second system to run, and events and jobs can no longer be committed in one transaction.

## Decision

We will keep Relay in **one monorepo**, write the core in **C# on .NET 10 LTS**, and store everything in **PostgreSQL**.

- The dashboard uses TypeScript, React and Vite. Python is used only where it suits the job best (the CLI, the SDK and load tests).
- Tool versions are pinned in the repo: `global.json`, `.nvmrc` and `.python-version`.
- Line endings are fixed by `.gitattributes` and `.editorconfig`, so macOS and Windows produce identical files.

The monorepo wins because the parts share contracts and there is one developer, so coordination cost matters more than independent release cycles. .NET wins because the hard part of Relay is reliable background work, and .NET's hosting model, typed HTTP client and cancellation support cover most of what that needs. Postgres wins because one durable store keeps "store the event and create its delivery job" a single transaction.

## Consequences

- **Easier:** changing an API contract and its consumers together, onboarding (one clone and one README), and keeping CI in a single place.
- **Easier:** at-least-once delivery, because the event and its job are written in the same transaction (see ADR-0003 and ADR-0004).
- **Harder:** CI will need path filters once the dashboard and Python parts exist, so a dashboard change doesn't rebuild the API.
- **Harder:** three toolchains (.NET, Node, Python) on two operating systems. Pinned versions reduce drift but have to be bumped deliberately.
- **Risk:** Postgres is both the database and the queue, so queue load competes with ingestion for connections and I/O. Phase 11 load tests measure this.
- **Revisit:** split a part into its own repo if it gains its own contributors or release cycle. Move the queue off Postgres if measurements show contention we can't fix with indexes, batching or pool sizing.
