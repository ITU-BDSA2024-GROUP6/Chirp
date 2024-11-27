using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Data;
using Chirp.Core.RepositoryInterfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Chirp.Core.Models;


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
    DbInitializer.SeedDatabase(chatDBContext);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
/*
// Add security headers
app.UseSecurityHeaders(new HeaderPolicyCollection()
    .AddDefaultSecurityHeaders()
    .AddStrictTransportSecurityMaxAgeIncludeSubDomains()
    .AddContentSecurityPolicy(builder =>
    {
        builder.AddDefaultSrc().Self();
        builder.AddScriptSrc().Self().UnsafeInline();
        builder.AddStyleSrc().Self().UnsafeInline();
        builder.AddImgSrc().Self().Data();
        builder.AddFormAction().Self();
        builder.AddConnectSrc().Self();
        // if we need to allow GitHub authentication:
        // builder.AddFrameSrc().From("https://github.com");
    }));
*/

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();