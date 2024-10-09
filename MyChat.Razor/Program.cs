using MyChat.Razor.Repositories;


var builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ChatDBContext>(options => options.UseSqlite(connectionString));


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<ICheepRepository, CheepRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();



var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var chatDBContext = scope.ServiceProvider.GetRequiredService<ChatDBContext>();
    DbInitializer.SeedDatabase(chatDBContext);
    var authorRepository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
    
    // Create a new author
    string testAuthorName = "Test Author";
    string testAuthorEmail = "test.author@example.com";

    bool authorCreated = authorRepository.createAuthor(testAuthorName, testAuthorEmail);
    
    if (authorCreated)
    {
        Console.WriteLine($"Successfully created author: {testAuthorName} with email: {testAuthorEmail}");
    }
    else
    {
        Console.WriteLine($"Failed to create author: {testAuthorName}. Email may already exist.");
    }
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
