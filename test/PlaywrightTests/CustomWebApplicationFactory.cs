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
using Chirp.Core.Models;

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
        // Before building the initial host, we need to modify the configuration
        builder.ConfigureServices(services =>
        {
            // Remove ALL existing authentication and identity services
            var descriptorsToRemove = services
                .Where(d => 
                    d.ServiceType.Namespace?.Contains("Authentication") == true ||
                    d.ServiceType.Namespace?.Contains("Identity") == true ||
                    d.ServiceType.Namespace?.Contains("Security") == true
                ).ToList();
            
            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Add our test authentication setup
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "TestAuth";
                options.DefaultSignInScheme = "TestAuth";
            }).AddCookie("TestAuth");

            // Re-add Identity with minimal configuration
            services.AddIdentityCore<Author>(options => 
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 3; // Minimal for testing
            })
            .AddEntityFrameworkStores<ChatDBContext>();

            // Configure GitHub auth to be disabled
            services.Configure<AuthenticationOptions>(options =>
            {
                options.DefaultChallengeScheme = "TestAuth";
            });

            // Rest of your existing database configuration
            var existingDbContext = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ChatDBContext>));
            if (existingDbContext != null)
            {
                services.Remove(existingDbContext);
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
        });

        // Continue with the rest of your existing code
        var initialHost = builder.Build();
        var selectedPort = GetAvailablePort();
        var baseUrl = $"http://127.0.0.1:{selectedPort}";

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
        Thread.Sleep(1500); // Allow time for proper shutdown
        _testHost?.Dispose();
    }
}