var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("Relay");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:Relay is not configured.\n" +
        "In development, run:\n" +
        "dotnet user-secrets set \"ConnectionStrings:Relay\" " +
        "\"<Npgsql connection string>\" --project services/api/Relay.Api\n" +
        "In other environments, set ConnectionStrings__Relay."
    );
}

builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");

app.Run();
