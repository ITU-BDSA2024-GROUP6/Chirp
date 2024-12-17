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

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ChatDBContext>(options => options.UseSqlite(connectionString));

// Add Identity services
builder.Services.AddDefaultIdentity<Author>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ChatDBContext>();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<ICheepRepository, CheepRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultChallengeScheme = "GitHub";
    })
    .AddCookie()
    .AddGitHub(o =>
    {
        o.ClientId = builder.Configuration["authentication_github_clientId"] ?? "";
        o.ClientSecret = builder.Configuration["authentication_github_clientSecret"] ?? "";
        o.Scope.Add("read:user");
        o.Scope.Add("user:email");
        o.CallbackPath = "/signin-github";
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var chatDBContext = scope.ServiceProvider.GetRequiredService<ChatDBContext>();
    chatDBContext.Database.EnsureCreated();
    DbInitializer.SeedDatabase(chatDBContext);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Configure static files with proper MIME types
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".css"] = "text/css";
provider.Mappings[".js"] = "application/javascript";
provider.Mappings[".map"] = "application/json";
// Add specific mapping for Razor-generated CSS
provider.Mappings[".cshtml.css"] = "text/css";

// Configure static files BEFORE security headers
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider,
    ServeUnknownFileTypes = true // This helps with some edge cases
});

app.UseHttpsRedirection();

// Security headers AFTER static files
app.UseHsts(hsts => hsts.MaxAge(365).IncludeSubdomains());
app.UseXContentTypeOptions();
app.UseReferrerPolicy(opts => opts.NoReferrer());
app.UseXXssProtection(options => options.EnabledWithBlockMode());
app.UseXfo(options => options.Deny());

app.UseCsp(opts => opts
    .DefaultSources(s => s.Self())
    .ScriptSources(s => s.Self().UnsafeInline().CustomSources("https://cdnjs.cloudflare.com"))
    .StyleSources(s => s.Self().UnsafeInline())
    .ImageSources(s => s.Self().CustomSources("data:"))
    .FormActions(s => s
        .Self()
        .CustomSources(
            "https://github.com",
            "https://github.com/login/oauth/authorize"
        )
    )
    .FrameAncestors(s => s.Self())
    .BaseUris(s => s.Self())
    .FrameSources(s => s.CustomSources("https://github.com"))
    .ConnectSources(s => s.Self())
);

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();

//This makes the program public, then the test class can access it
public partial class Program { }