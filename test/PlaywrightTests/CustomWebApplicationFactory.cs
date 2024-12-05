using System.Data.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Chirp.Infrastructure.Data;
using Chirp.Web;

namespace test.PlaywrightTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private IHost? _testHost;
    // We maintain a pool of ports to avoid conflicts when running multiple tests
    private static readonly Queue<int> PortPool = new Queue<int>(Enumerable.Range(4000, 15));

    private static int GetAvailablePort()
    {
        lock (PortPool)
        {
            if (PortPool.Count > 0)
            {
                return PortPool.Dequeue();
            }
            throw new InvalidOperationException("No available ports remaining in the pool.");
        }
    }

    public string TestServerAddress
    {
        get
        {
            if (_testHost is null)
            {
                using var client = CreateDefaultClient();
            }
            return ClientOptions.BaseAddress.ToString();
        }
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var initialHost = builder.Build();
        var selectedPort = GetAvailablePort();
        var baseUrl = $"http://127.0.0.1:{selectedPort}";

        builder.ConfigureServices(services =>
        {
            // First, remove any existing database configurations
            var existingDbContext = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ChatDBContext>));
            if (existingDbContext != null)
            {
                services.Remove(existingDbContext);
            }

            var dbConnDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbConnection));
            if (dbConnDescriptor != null)
            {
                services.Remove(dbConnDescriptor);
            }

            // Set up our test database connection
            services.AddSingleton<DbConnection>(_ =>
            {
                var sqliteConn = new SqliteConnection("DataSource=:memory:");
                sqliteConn.Open();
                return sqliteConn;
            });

            services.AddDbContext<ChatDBContext>((provider, options) =>
            {
                var conn = provider.GetRequiredService<DbConnection>();
                options.UseSqlite(conn);
            });

            // Remove the existing authentication configuration
            var authBuilder = services.SingleOrDefault(
                d => d.ServiceType == 
                    typeof(Microsoft.AspNetCore.Authentication.AuthenticationBuilder));
            if (authBuilder != null)
            {
                services.Remove(authBuilder);
            }

            // Set up simplified authentication for testing
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Identity.Application";
                options.DefaultSignInScheme = "Identity.External";
                options.DefaultChallengeScheme = "Identity.External";
            })
            .AddCookie("Identity.Application")
            .AddCookie("Identity.External");
        });

        // Use the Development environment for testing
        builder.UseEnvironment("Development");
        
        // Configure the test server
        builder.ConfigureWebHost(webHostBuilder => 
            webHostBuilder.UseKestrel().UseUrls(baseUrl));

        _testHost = builder.Build();
        _testHost.Start();

        // Set up the client options with the correct server address
        var server = _testHost.Services.GetRequiredService<IServer>();
        var addressesFeature = server.Features.Get<IServerAddressesFeature>() ?? 
            throw new InvalidOperationException("No server addresses available.");
        ClientOptions.BaseAddress = addressesFeature.Addresses
            .Select(uri => new Uri(uri)).Last();

        initialHost.Start();
        return initialHost;
    }

    protected override void Dispose(bool disposing)
    {
        _testHost?.StopAsync().Wait();
        Thread.Sleep(1500); // Allow time for proper shutdown
        _testHost?.Dispose();
    }
}