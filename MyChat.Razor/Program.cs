using MyChat.Razor.Repositories;


var builder = WebApplication.CreateBuilder(args);

var connectionString = string.Empty;

if (builder.Environment.IsDevelopment())
{
    // Local development database path
    connectionString = Path.Combine(AppContext.BaseDirectory, "App_Data", "Chat.db");
}
else
{
    // Azure environment database path
    connectionString = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "Chat.db");
}
Console.WriteLine($"Database path: {connectionString}");
builder.Services.AddDbContext<ChatDBContext>(options => options.UseSqlite($"Data Source={connectionString}"));


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<ICheepRepository, CheepRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();



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

app.MapRazorPages();

app.Run();
