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
/* Custom test environment for tests in ASP.NET Core with Playwright.
Defines a custom factory for the test server environment for the application
Referenced from: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0
 */
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private IHost? _testHost;
    private static readonly Queue<int> PortPool = new Queue<int>(Enumerable.Range(4000, 15));  // Set of available ports, e.g., 4000-4014

    // Get the next available port from the queue
    private static int GetAvailablePort()
    {
        lock (PortPool)
        {
            if (PortPool.Count > 0)
            {
                return PortPool.Dequeue();  // Dequeue the next port
            }
            throw new InvalidOperationException("No available ports remaining in the pool.");
        }
    }

    // Property to access the base URL of the test server
    public string TestServerAddress
    {
        get
        {
            if (_testHost is null)
            {
                // Forces WebApplicationFactory to initialize the server
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
            // Your existing database configuration
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

            // Add this new part to handle GitHub auth in test environment
            services.Configure<AuthenticationOptions>(options =>
            {
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            });
        });

        builder.UseEnvironment("Development");
        builder.ConfigureWebHost(webHostBuilder => webHostBuilder.UseKestrel().UseUrls(baseUrl));

        _testHost = builder.Build();
        _testHost.Start();

        var server = _testHost.Services.GetRequiredService<IServer>();
        var addressesFeature = server.Features.Get<IServerAddressesFeature>() ?? 
            throw new InvalidOperationException("No server addresses available.");
        ClientOptions.BaseAddress = addressesFeature.Addresses.Select(uri => new Uri(uri)).Last();

        initialHost.Start();
        return initialHost;
    }

    protected override void Dispose(bool disposing)
    {
        _testHost?.StopAsync().Wait();
        Thread.Sleep(1500); // Give time for the server to shut down properly
        _testHost?.Dispose();
    }
}