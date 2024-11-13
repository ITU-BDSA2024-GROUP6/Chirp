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
        //options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        //options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        //options.DefaultChallengeScheme = "GitHub";
    })
    //.AddCookie()
    
    .AddGitHub(o =>
    {
        o.ClientId = builder.Configuration["authentication_github_clientId"];
        o.ClientSecret = builder.Configuration["authentication_github_clientSecret"];
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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
