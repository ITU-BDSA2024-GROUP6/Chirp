using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Data;
using Chirp.Core.RepositoryInterfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Chirp.Core.Models;
using Microsoft.AspNetCore.Builder;
using NWebsec.AspNetCore.Middleware;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Retrieve connection string for the database from configuration
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Add the database context using SQLite and the retrieved connection string
builder.Services.AddDbContext<ChatDBContext>(options => options.UseSqlite(connectionString));

// Add Identity services for managing user authentication and authorization
builder.Services.AddDefaultIdentity<Author>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ChatDBContext>(); // Use Entity Framework with the provided ChatDBContext for user data

// Register repositories to access data for Cheeps and Authors
builder.Services.AddScoped<ICheepRepository, CheepRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();

// Set up authentication services with Cookie and GitHub OAuth
builder.Services.AddAuthentication(options =>
    {
        options.DefaultChallengeScheme = "GitHub"; // Default scheme for authentication challenges
    })
    .AddCookie() // Adds cookie authentication
    .AddGitHub(o =>
    {
        // Configure GitHub OAuth settings using values from configuration
        o.ClientId = builder.Configuration["authentication_github_clientId"] ?? "";
        o.ClientSecret = builder.Configuration["authentication_github_clientSecret"] ?? "";
        o.Scope.Add("read:user");
        o.Scope.Add("user:email");
        o.CallbackPath = "/signin-github"; // Callback URL for GitHub OAuth
    });

var app = builder.Build();

// Ensures that the database is created and seeds initial data (if necessary)
using (var scope = app.Services.CreateScope())
{
    var chatDBContext = scope.ServiceProvider.GetRequiredService<ChatDBContext>();
    chatDBContext.Database.EnsureCreated(); // Ensures the database schema is created
    DbInitializer.SeedDatabase(chatDBContext); // Seed the database with initial data
}

// Configure the HTTP request pipeline for different environments
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error"); // Show error page for non-development environments
    app.UseHsts(); // Enable HTTP Strict Transport Security (HSTS) for enhanced security
}

// Configure static file handling with proper MIME type mappings
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".css"] = "text/css";
provider.Mappings[".js"] = "application/javascript";
provider.Mappings[".map"] = "application/json";
// Add specific mapping for Razor-generated CSS
provider.Mappings[".cshtml.css"] = "text/css";

// Serve static files with custom content type provider and allow unknown file types
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider,
    ServeUnknownFileTypes = true // Allow serving unknown file types in certain edge cases
});

// Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// Configure security headers for content security policy (CSP) and other headers
app.UseHsts(hsts => hsts.MaxAge(365).IncludeSubdomains()); // Enforce HSTS with a max age of 365 days
app.UseXContentTypeOptions(); // Prevent browsers from interpreting files as a different MIME type
app.UseReferrerPolicy(opts => opts.NoReferrer()); // Prevent sending referrer headers
app.UseXXssProtection(options => options.EnabledWithBlockMode()); // Enable cross-site scripting protection
app.UseXfo(options => options.Deny()); // Prevent framing of the page

// Configure Content Security Policy (CSP) for script, style, image, and form actions
app.UseCsp(opts => opts
    .DefaultSources(s => s.Self()) // Allow content from the same origin
    .ScriptSources(s => s.Self().UnsafeInline().CustomSources("https://cdnjs.cloudflare.com")) // Allow scripts from self and a CDN
    .StyleSources(s => s.Self().UnsafeInline()) // Allow styles from self and inline styles
    .ImageSources(s => s.Self().CustomSources("data:")) // Allow images from self and data URIs
    .FormActions(s => s
        .Self() // Allow form submissions to the same origin
        .CustomSources("https://github.com", "https://github.com/login/oauth/authorize")) // Allow form actions to GitHub
    .FrameAncestors(s => s.Self()) // Allow framing from the same origin
    .BaseUris(s => s.Self()) // Allow base URIs from the same origin
    .FrameSources(s => s.CustomSources("https://github.com")) // Allow framing from GitHub
    .ConnectSources(s => s.Self()) // Allow connections from the same origin
);

// Set up request routing, authentication, and authorization middleware
app.UseRouting();
app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization(); // Add authorization middleware
app.MapRazorPages(); // Map Razor Pages for handling web requests
app.Run(); // Start the web application

// This partial class allows for unit testing of the Program class by exposing the app
public partial class Program { }
