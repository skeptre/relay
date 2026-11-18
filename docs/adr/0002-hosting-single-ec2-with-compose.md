# ADR-0002: Host v1 on a single EC2 instance with Docker Compose and Caddy

- **Status:** Accepted
- **Date:** 2026-11-18

## Context

Relay needs a public HTTPS URL. Webhook senders (GitHub, Stripe) must reach it, and it has to be live to show on skepslabs.com. Merges to `main` should deploy automatically.

Constraints:

- One developer paying out of pocket, so monthly cost matters.
- Traffic in v1 is demo and portfolio scale: a handful of endpoints and occasional bursts from load tests.
- The setup should be something I can operate and explain, not a platform I configure by guesswork.
- Local development already runs Postgres in Docker Compose, and the API ships as a container image (step 2.1).

## Options considered

Costs are rough on-demand list prices in us-east-1. Other regions are a little higher.

1. **Single EC2 instance (t3.small) running Docker Compose: Caddy, API and Postgres.** About $20/month: instance about $15, public IPv4 about $3.60, 20 GB gp3 about $1.60.
   - Pro: the same Compose model I use locally. One machine to reason about. Caddy issues and renews TLS certificates automatically.
   - Con: a single point of failure. Postgres backups and OS patching are my job.
2. **ECS Fargate + RDS Postgres + Application Load Balancer.** About $45–60/month: ALB about $18, RDS db.t4g.micro about $15, Fargate task about $10, plus IPv4 and storage.
   - Pro: managed database with automated backups, rolling deploys, no servers to patch.
   - Con: two to three times the cost. Many more moving parts (VPC, subnets, target groups, task definitions, IAM), and most of the learning goes to AWS configuration, not to Relay.
3. **A PaaS (Fly.io, Render, Railway).**
   - Pro: least setup.
   - Con: the project brief says AWS, and it hides exactly the deployment concepts this phase is meant to teach.

## Decision

We will run v1 on **one EC2 instance with Docker Compose**:

- **Caddy** terminates TLS on ports 80 and 443 and reverse-proxies to the API.
- **The API** runs as the image built in CI, published to GitHub Container Registry and tagged with the commit SHA.
- **Postgres 18** runs in a container with its data on a named volume. It is never exposed on a host port.

Deploys run from GitHub Actions after CI passes on `main`: build and push the image, then SSH to the instance, pull, and `docker compose up -d`. The job then checks `https://<domain>/health` and fails if it doesn't return 200. Production secrets live only on the instance (`/opt/relay/.env`). CI never sees them.

This wins because v1 needs to be cheap, live and understood. The single-instance setup costs about a third as much as the managed option, and every component is visible with `docker compose ps`.

## Consequences

- **Easier:** one Compose model for local and production. A deploy is "pull the image, restart". Rollback is "point at the previous SHA". Caddy handles certificates, so there is no manual TLS work.
- **Harder: availability.** If the instance or its disk dies, Relay is down until it's rebuilt. That's acceptable for v1, and stated honestly in the README.
- **Harder: data safety is on me.** Postgres lives on the instance's EBS volume. Daily EBS snapshots (Data Lifecycle Manager) are switched on at provisioning, and Phase 12 adds `pg_dump` backups and a tested restore.
- **Harder: patching.** Ubuntu unattended-upgrades applies security patches. Image tags are pinned and bumped deliberately.
- **Risk: SSH is open to the internet** for GitHub-hosted runners. It is key-only, uses a dedicated deploy user, and the host key is pinned in CI. Upgrade path: deploy through AWS SSM Run Command with GitHub OIDC, and close port 22.
- **Risk: one box shares CPU, memory and I/O** between ingestion, delivery workers and Postgres. Phase 11 load tests measure where it saturates.
- **Revisit when:** sustained load outgrows a single instance, uptime starts to matter (paying users), or the database needs point-in-time recovery. The next step is RDS for Postgres first, then ECS or multiple API instances behind a load balancer. The API is already stateless, so moving it is mostly configuration.
