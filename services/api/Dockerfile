# Relay API image. Build from the repo root so global.json is in the context:
#   docker build -f services/api/Dockerfile -t relay-api .

# ---------- build ----------
# SDK pinned to an exact patch inside the band global.json allows (10.0.4xx, latestPatch).
FROM mcr.microsoft.com/dotnet/sdk:10.0.404 AS build
WORKDIR /src

# Restore first, using only the files that affect it, so this layer stays cached
# until a package reference changes.
COPY global.json ./
COPY services/api/Relay.Api/Relay.Api.csproj services/api/Relay.Api/
RUN dotnet restore services/api/Relay.Api/Relay.Api.csproj

COPY services/api/Relay.Api/ services/api/Relay.Api/
RUN dotnet publish services/api/Relay.Api/Relay.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

# ---------- runtime ----------
# Chiseled: no shell, no package manager, runs as non-root ("app", UID 1654) by default.
FROM mcr.microsoft.com/dotnet/aspnet:10.0.15-noble-chiseled AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish ./

# ConnectionStrings__Relay must be provided at run time; the app fails fast without it.
# Run with an init process (docker run --init, compose `init: true`, ECS initProcessEnabled).
# Without one, dotnet is PID 1, a startup crash's SIGABRT is ignored, and the container hangs.
ENTRYPOINT ["dotnet", "Relay.Api.dll"]
