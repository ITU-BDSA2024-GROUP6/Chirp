// Import necessary namespaces
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using SimpleDB;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


// Return chirps from the database
app.MapGet("/chirps", (int? limit, int? offset) => {
    limit = limit ?? 50; // Default to 50 if not specified
    offset = offset ?? 0; // Default to 0 if not specified
    
    var chirps = CSVDatabase<Chirp>.Instance.Read()
        .Skip(offset.Value)
        .Take(limit.Value)
        .ToList();
    
    return Results.Ok(chirps);
});

// Save a new chirp to the database
app.MapPost("/chirp", (Chirp chirp) =>
{
    CSVDatabase<Chirp>.Instance.Store(chirp);
    return Results.Ok("Chirp saved successfully!");
});

app.Run();