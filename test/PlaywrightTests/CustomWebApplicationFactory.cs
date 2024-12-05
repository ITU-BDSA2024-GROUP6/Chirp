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
        var initialHost = builder.Build();
        var selectedPort = GetAvailablePort();
        var baseUrl = $"http://127.0.0.1:{selectedPort}";

        builder.ConfigureServices(services =>
        {
            // First, remove any existing authentication configuration
            var descriptors = services.Where(d => 
                d.ServiceType.Name.Contains("Authentication") ||
                d.ServiceType.Name.Contains("Security") ||
                d.ServiceType.Name.Contains("GitHub") ||
                d.ServiceType == typeof(IAuthenticationService) ||
                d.ServiceType == typeof(IAuthenticationHandlerProvider)
            ).ToList();
            
            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Configure test authentication
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "TestAuth";
                options.DefaultAuthenticateScheme = "TestAuth";
                options.DefaultChallengeScheme = "TestAuth";
                options.DefaultSignInScheme = "TestAuth";
            }).AddCookie("TestAuth");

            // Add Identity without the default UI (since we're testing)
            services.AddIdentityCore<Author>(options => 
            {
                options.SignIn.RequireConfirmedAccount = false;
                // Make password requirements simple for testing
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<ChatDBContext>();

            // Database configuration (your existing code)
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
        Thread.Sleep(1500); // Allow time for proper shutdown
        _testHost?.Dispose();
    }
}