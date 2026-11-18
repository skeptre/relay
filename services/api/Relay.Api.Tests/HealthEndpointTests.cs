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
                builder.UseSetting("ConnectionStrings:Relay", "Host=127.0.0.1;Port=1;Database=x;Username=x;Password=x;Timeout=2"));
        var client = factory.CreateClient();
        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("Unhealthy", body);
    }
}
