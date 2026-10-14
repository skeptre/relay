# Learnings

## `.gitattributes`

- Controls how Git handles files.
- Commonly used for:
  - Line-ending normalization
  - File type handling
  - Merge behaviour
  - Diff behaviour

## `.editorconfig`

- Controls how editors such as VS Code format and save files.
- Commonly used for:
  - Line endings
  - Indentation
  - Character encoding
  - Trailing whitespace
  - Final newlines

## Important

`.gitattributes` and `.editorconfig` should use compatible settings.

For example:

- `.editorconfig` may tell VS Code to save files using `LF`.
- `.gitattributes` may tell Git to normalize files to `LF`.

If they are configured inconsistently, files can appear as modified because the editor and Git are handling line endings differently.

## Mental Model

```text
.editorconfig  → controls how the editor writes files
.gitattributes → controls how Git handles those files
```

## Mistakes made

```text
eol=LF in uppercase, it's silently ignored
.cmd files are stored as LF in repo but disk gets CRLF
*.png text would corrupt every file by changing top level header bytes 10-13
```

## `global.json`

- Pinning to whatever version is installed is not a decision. Check that the version is still supported and patched.

## SDK feature bands

latestPatch keeps the version strictly within the same feature band, for example if we have version 4.0.100, it will rollForward to 4.0.105 but won't do 4.0.200

latestFeature instead will rollForward to 4.0.200 or 4.0.305, as long as it stays within that minor version and if the version is already installed.

4 is major, 0 is minor, Feature Band is 1 and Patch 05. 4.0.105

When no version in the band is installed, the build fails loudly.

## .python-version vs requires-python

Python interpreter version can be pinned and is different from allowed range of version that users can install  

## Architecture Decision Record

ADRs are immutable. Supersede, don't edit the current version. We take the template and write the new updates on it, in the end we have multiple version of each major decision and why we made such decision.

## Store first, then return 2xx

The idea is that we must store the event first before returning a positive http code. If we respond before saving, the event can be lost.

## Step 0.1

This step set up the tools and initial configuration.

Same line endings makes sure files don't change on different machines.

Version pinning ensures compatibility and allows builds to succeed without failing silently.

Docs ADR, we have a what and why of each major decision.

## Step 0.2 + 0.3

Built `/health` that checks Postgres, not just that the API process is running.

## Health checks

- `/health` with no checks only proves the process is alive. It can say 200 while the DB is broken. That's a false positive.
- `AddHealthChecks()` returns a builder. Packages add checks to it with extension methods (`AddNpgSql`), which is why they chain.
- On each request, every check runs. Npgsql opens a connection and runs a simple query. Healthy → 200, Unhealthy → 503.
- The check's name shows up in logs and must be unique. Default for `AddNpgSql` is `npgsql`.
- Docker's `pg_isready` only checks Postgres is accepting connections. It never logs in. That's why Docker said healthy while my API said 503.
- Later (Phase 2): split liveness and readiness. If a platform restarts the app whenever /health fails, a DB outage causes a restart loop.

## Fail fast vs. degrade

- Missing config can't fix itself, so crash at startup with a clear message.
- A DB outage can recover, so stay up, return 503, and recover when it comes back.
- Same idea in Compose: `${VAR:?message}` stops `docker compose up` if a variable is missing.
- A good error message says what's missing and how to fix it. Never put secret values in it.

## Configuration

- Order: appsettings.json → appsettings.{Env}.json → user secrets → env vars → command line. Later ones win.
- User secrets only load in Development. `--no-launch-profile` means no launchSettings.json, so the environment is Production and the secret is ignored.
- In env vars, `:` becomes `__`, e.g. `ConnectionStrings__Relay`.
- User secrets are stored per machine and aren't encrypted. They just keep secrets out of the repo.

## Postgres in Docker

- `POSTGRES_*` env vars are only used the first time the container starts with an empty volume. After that, changing `.env` does nothing.
- `down` keeps the volume, `down -v` deletes it and all the data.
- Port 5432 is Postgres (its own protocol), port 5080 is my API (HTTP). I only see Postgres through the API.

## Debugging a connection problem

1. Is anything listening on the port? (`netstat`, `docker compose ps`)
2. Did the request reach the server? Check the server's own logs.
3. Do the client and server agree? (credentials, DB name)
4. Was the setting actually applied? (init-once settings)

Fix every cause you found before retesting.

## Git: rewriting history

- `commit --amend` only fixes the last commit. `rebase -i` with `fixup` merges older commits.
- Make a backup branch first, after committing everything.
- After a rebase, the code should be identical. Only the commits change.
- Pushing rewritten history needs `--force-with-lease`.
- `main` and `origin/main` are different. Pull after a PR merges.
- `git branch -D` only deletes the local branch. Delete the remote branch on GitHub, then run `git fetch --prune`.

## Windows notes

- A running `.exe` is locked, so `dotnet build` fails while the API is running.
- In PowerShell, use single quotes for secrets because double quotes expand `$`. Use `curl.exe`, not `curl`.
- VS Code Timeline can bring back discarded changes.

## Mistakes made in this session

```text
Error message: --project ended up inside the quotes, so the command wouldn't run. Read escaped strings as their printed output.
Thought "Healthy" worked, but that was before AddNpgSql, so it only proved the process was alive.
Password mismatch, plus the volume kept the old password. Reset the volume but forgot the secret, so it failed again.
Backup branch made before amending, so the verification diff wasn't empty. Check what your check is comparing.
Switched to main without pulling, so the files "disappeared".
```
