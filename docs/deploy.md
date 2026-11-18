# Deploying Relay

Production is one EC2 instance running Docker Compose: Caddy (TLS), the API, and Postgres. See [ADR-0002](adr/0002-hosting-single-ec2-with-compose.md) for why.

```text
GitHub ──push──▶ main ──CI passes──▶ Deploy workflow
                                       │ build image → ghcr.io/<owner>/relay-api:<sha>
                                       │ scp compose.yaml + Caddyfile
                                       │ ssh: pull, up -d
                                       ▼
             EC2 (Ubuntu 24.04, /opt/relay)
             ┌──────────────────────────────────────────────┐
 :443 ──────▶│ caddy ──▶ api:8080 ──▶ postgres:5432 (volume) │
             └──────────────────────────────────────────────┘
```

| File | Where it lives | Who writes it |
|---|---|---|
| `infra/prod/compose.yaml`, `Caddyfile` | repo → copied to `/opt/relay` on every deploy | the repo |
| `/opt/relay/.env` | server only, `chmod 600` | you, once |
| `/opt/relay/release.env` | server only | the deploy workflow (the image tag) |

## One-time setup

### 1. Deploy key

On your machine:

```bash
ssh-keygen -t ed25519 -f ~/.ssh/relay_deploy -C relay-deploy -N ""
cat ~/.ssh/relay_deploy.pub
```

This key is used only by GitHub Actions. It's separate from your personal admin key.

### 2. Launch the instance (AWS console → EC2 → Launch instance)

| Setting | Value |
|---|---|
| AMI | Ubuntu Server 24.04 LTS, 64-bit (x86) |
| Type | `t3.small` |
| Key pair | your personal key (logs in as `ubuntu`, for admin) |
| Storage | 20 GiB gp3 |
| Security group | inbound TCP 22, TCP 80, TCP 443 and UDP 443 from anywhere |
| User data | contents of [`infra/prod/cloud-init.yaml`](../infra/prod/cloud-init.yaml), with `REPLACE_WITH_DEPLOY_PUBLIC_KEY` replaced by the middle part of `relay_deploy.pub` |

Port 22 is open because GitHub-hosted runners have no fixed IP. Only key logins are accepted. See ADR-0002 for the upgrade path (SSM).

### 3. Static IP and DNS

1. EC2 → Elastic IPs → Allocate, then associate it with the instance.
2. At your DNS provider, add an **A record**: `relay.skepslabs.com` → the Elastic IP. Use a low TTL (e.g. 300) at first.
3. Wait until `dig +short relay.skepslabs.com` returns the IP. Caddy can't get a certificate until it does.

### 4. Backups (do this now, not later)

EC2 → Lifecycle Manager → Create policy: EBS snapshot policy, target the instance's volume (by tag), daily, keep 7. This is a stopgap. Phase 12 adds `pg_dump` backups and a tested restore.

### 5. Finish the server

```bash
ssh ubuntu@<elastic-ip>
cloud-init status --wait          # should end in "status: done"
docker --version && docker compose version

# Production secrets: written once, never leave the server.
sudo -u deploy tee /opt/relay/.env >/dev/null <<EOF
RELAY_DOMAIN=relay.skepslabs.com
ACME_EMAIL=<your email>
POSTGRES_USER=relay
POSTGRES_PASSWORD=$(openssl rand -base64 32 | tr -d '/+=')
POSTGRES_DB=relaydb
EOF
sudo chmod 600 /opt/relay/.env
```

### 6. Pin the host key

The first time you SSH in, check the host key fingerprint against EC2 → instance → Actions → Monitor → Get system log (look for `BEGIN SSH HOST KEY FINGERPRINTS`). Then capture it:

```bash
ssh-keyscan -t ed25519 <elastic-ip>
```

### 7. GitHub configuration

**Settings → Secrets and variables → Actions → Variables (repository):**

| Variable | Value |
|---|---|
| `RELAY_DOMAIN` | `relay.skepslabs.com` |

It has to be a repository variable, not an environment one, because the workflow checks it before the job's environment loads.

**Settings → Environments → New environment `production` → Environment secrets:**

| Secret | Value |
|---|---|
| `DEPLOY_HOST` | the Elastic IP |
| `DEPLOY_SSH_KEY` | full contents of `~/.ssh/relay_deploy` (the **private** key) |
| `DEPLOY_KNOWN_HOSTS` | the `ssh-keyscan` output line from step 6 |

Optional: under the environment, set **Deployment branches** to `main` only.

### 8. First deploy

Actions → Deploy → **Run workflow** (branch `main`). The last step polls `https://relay.skepslabs.com/health` and passes on 200. After that, every merge to `main` deploys on its own once CI is green.

## Day-to-day

```bash
ssh ubuntu@<elastic-ip>
sudo -iu deploy                   # /opt/relay belongs to the deploy user
cd /opt/relay
c="docker compose --env-file .env --env-file release.env"

$c ps                     # what's running
$c logs -f --tail 100 api # API logs
$c logs caddy | grep -i acme   # certificate issues
$c exec postgres psql -U relay relaydb
```

### Rollback

Either option works:

- **From GitHub:** Actions → Deploy → open the last good run → **Re-run all jobs**. It redeploys that run's commit.
- **On the server:** set `RELAY_API_IMAGE` in `release.env` to the previous `ghcr.io/...:<sha>`, then run `$c pull api && $c up -d api`.

Schema migrations (Phase 1.2+) make rollback harder: an old image may not understand a newer schema. Keep migrations backward-compatible for one release (add before you remove).

## Costs (approximate, us-east-1)

| Item | ~USD/month |
|---|---|
| t3.small on-demand | 15 |
| Public IPv4 (Elastic IP) | 3.60 |
| 20 GiB gp3 + snapshots | 2–3 |
| **Total** | **~21** |

Set a billing alarm: Billing → Budgets → monthly budget of $30 with an email alert.
