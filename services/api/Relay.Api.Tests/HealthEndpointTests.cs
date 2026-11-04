using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Relay.Api.Tests;

public class HealthEndpointTests
{
    [Fact]
    public async Task Health_Returns503_WhenDatabaseIsUnreachable()
    {
        using var factory = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder =>
        builder.UseSetting("ConnectionStrings:Relay", "<fake string"))
    }
}
