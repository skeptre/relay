# PROGRESS.md — Relay

> I update this file myself at the end of each step, using the update block Claude gives me. Commit it with the step's work so progress syncs between Mac and Windows.

## Current position

- **Phase:** 0 — Foundations
- **Step:** 0.1 — Repo structure and tooling files
- **Branch:** chore/0.1-repo-tooling (pushed)
- **Status:** In progress — `master` renamed to `main`; `.gitattributes` and `.editorconfig` done
- **Blocked on:** nothing
- **Next action:** Mac environment check, then pin tool versions (Node LTS, choose Python version)

## Environment

| | macOS | Windows |
|---|---|---|
| Git | | 2.56.0 (`core.autocrlf=true`) |
| .NET SDK (pinned in `global.json`) | | 10.0.202 |
| Node (pinned in `.nvmrc`) | | 25.9.0 — not LTS, must change |
| Python (pinned in `.python-version`) | | `python` 3.12.7, `py` 3.14, uv 3.13 — must decide |
| Docker Desktop | | 29.8.1 (WSL2 backend) |
| VS Code + Claude Code extension | | ✓ (EditorConfig extension installed) |
| Repo cloned and running | ☐ | ☐ |

## Roadmap checklist

### Phase 0 — Foundations
- [ ] 0.1 Repo structure, `.gitignore`, `.gitattributes`, `.editorconfig`, pinned versions, README stub, ADR template, `LEARNINGS.md`
- [ ] 0.2 ASP.NET Core API with `/health`
- [ ] 0.3 Docker Compose with Postgres; `/health` reports DB status
- [ ] 0.4 React + Vite + TypeScript dashboard scaffold
- [ ] 0.5 GitHub Actions CI for API and dashboard
- [ ] 0.6 ADR-001: monorepo and stack
- [ ] Phase checkpoint (explain-back, interview questions, patterns)

### Phase 1 — Walking skeleton
- [ ] 1.1 Design `endpoints` and `events` (with `owner_id`)
- [ ] 1.2 EF Core setup and first migration
- [ ] 1.3 `POST /endpoints`
- [ ] 1.4 `ANY /in/{slug}` ingestion returning 202
- [ ] 1.5 `GET /endpoints/{id}/events`
- [ ] 1.6 Minimal React events page
- [ ] 1.7 First integration test
- [ ] Phase checkpoint

### Phase 2 — First deploy and CD
- [ ] 2.1 Multi-stage Dockerfile
- [ ] 2.2 AWS setup chosen and provisioned (ADR-002)
- [ ] 2.3 Domain, HTTPS, env vars and secrets
- [ ] 2.4 Deploy on merge to `main`
- [ ] 2.5 Dashboard built and served
- [ ] Phase checkpoint

### Phase 3 — Ingestion done properly
- [ ] 3.1 Request size limits
- [ ] 3.2 Headers and binary bodies, any content type
- [ ] 3.3 Source IP behind proxy
- [ ] 3.4 Cursor pagination
- [ ] 3.5 ProblemDetails errors
- [ ] 3.6 Edge-case tests
- [ ] Phase checkpoint

### Phase 4 — Delivery engine ★
- [ ] 4.1 Delivery design session (ADR-003, ADR-004)
- [ ] 4.2 Job created in same transaction as event
- [ ] 4.3 `BackgroundService` worker with `SKIP LOCKED` and leases
- [ ] 4.4 HTTP delivery with timeout, event ID header, attempts recorded
- [ ] 4.5 Retry policy (pure, unit tested)
- [ ] 4.6 Lease expiry proven by test
- [ ] 4.7 Concurrent workers proven by test
- [ ] 4.8 SSRF protection with tests
- [ ] 4.9 Graceful shutdown
- [ ] Phase checkpoint

### Phase 5 — Replay, dead letters and operations
- [ ] 5.1 Single event replay
- [ ] 5.2 Bulk replay of dead events
- [ ] 5.3 Endpoint pause/resume
- [ ] 5.4 Delivery history API
- [ ] 5.5 Retention job
- [ ] README: architecture diagram + delivery guarantees
- [ ] Phase checkpoint — **interview-ready core reached**

### Phase 6 — Accounts, auth and multi-tenancy
- [ ] 6.1 Auth approach chosen (ADR-005)
- [ ] 6.2 Dashboard sessions/tokens
- [ ] 6.3 Hashed, revocable API keys
- [ ] 6.4 Ownership enforcement + cross-tenant tests
- [ ] 6.5 Rate limiting
- [ ] Phase checkpoint

### Phase 7 — The real dashboard
- [ ] 7.1 Generated TS client in the build
- [ ] 7.2 Endpoints, endpoint detail, event detail, settings pages
- [ ] 7.3 Live updates via SSE (ADR-006)
- [ ] 7.4 Replay/pause controls and UI states
- [ ] 7.5 Vitest tests
- [ ] Phase checkpoint

### Phase 8 — Signatures
- [ ] 8.1 Inbound GitHub + Stripe verification
- [ ] 8.2 Constant-time comparison + timestamp tolerance
- [ ] 8.3 Outbound signing
- [ ] 8.4 Secret rotation
- [ ] 8.5 Python `verify()` SDK
- [ ] Phase checkpoint

### Phase 9 — CLI local forwarding
- [ ] 9.1 Protocol design
- [ ] 9.2 Server WebSocket endpoint + heartbeats
- [ ] 9.3 `relay listen` CLI
- [ ] 9.4 Reconnect with backoff
- [ ] 9.5 Packaging
- [ ] Phase checkpoint

### Phase 10 — Observability
- [ ] 10.1 Structured logs with correlation IDs
- [ ] 10.2 OpenTelemetry tracing
- [ ] 10.3 Metrics
- [ ] 10.4 Grafana dashboard
- [ ] 10.5 Health and readiness checks
- [ ] Phase checkpoint

### Phase 11 — Performance and resilience
- [ ] 11.1 Locust scenarios
- [ ] 11.2 Baseline measurements
- [ ] 11.3 Bottleneck fixes with before/after
- [ ] 11.4 Chaos tests, zero lost events
- [ ] 11.5 `docs/performance.md`
- [ ] Phase checkpoint

### Phase 12 — Production hardening
- [ ] 12.1 Security review
- [ ] 12.2 Backups + tested restore
- [ ] 12.3 Timeouts and limits
- [ ] 12.4 Error handling review
- [ ] 12.5 Runbook
- [ ] Phase checkpoint

### Phase 13 — Docs, launch and interview packaging
- [ ] 13.1 README complete
- [ ] 13.2 Architecture doc + ADR set
- [ ] 13.3 Demo video
- [ ] 13.4 Launch + top feedback fixed
- [ ] 13.5 Interview prep (pitch, deep dive, system design mock, stories)
- [ ] Phase checkpoint

## Decisions (ADR index)

| ADR | Title | Status | Date |
|---|---|---|---|
| | | | |

## Concepts checklist

Mark each: ☐ not yet · ◐ used it · ● can explain it in an interview

- ☐ HTTP semantics
- ☐ Idempotency
- ☐ Delivery guarantees
- ☐ Transactional outbox
- ☐ Transactions and isolation levels
- ☐ Row locking and `SKIP LOCKED`
- ☐ Leases / visibility timeouts
- ☐ Backoff with jitter
- ☐ SSRF
- ☐ HMAC, timing and replay attacks
- ☐ Authn vs authz, tenant isolation
- ☐ Rate limiting
- ☐ Cursor pagination
- ☐ WebSockets and SSE
- ☐ Contract-first APIs
- ☐ Logs / metrics / traces, percentiles
- ☐ Load testing, Little's Law
- ☐ Indexing and query plans
- ☐ Graceful shutdown
- ☐ CI/CD and containers
- ☐ ADRs

## Measured results (fill in during Phase 11)

| Metric | Value | Conditions |
|---|---|---|
| Sustained ingestion | | |
| Ingestion p99 latency | | |
| Delivery p99 latency | | |
| Events lost in chaos runs | | |

## Parked / later

- Add a `-text` rule to `.gitattributes` for raw HTTP test fixtures once the fixtures folder exists (Phase 3/8)

## Session log

| Date | Machine | Done | Next |
|---|---|---|---|
| 2026-09-30 | Windows | Renamed master → main; `.gitattributes` (fixed `eol=LF` case bug) and `.editorconfig` committed; branch pushed | Mac env check; pin tool versions |
