// Import necessary namespaces
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using SimpleDB;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Return chirps from the database
app.MapGet("/chirps", (int? limit) =>
{
    var chirps = CSVDatabase<Chirp>.Instance.Read(limit);
    return Results.Ok(chirps);
});

// Save a new chirp to the database
app.MapPost("/chirp", (Chirp chirp) =>
{
    CSVDatabase<Chirp>.Instance.Store(chirp);
    return Results.Ok("Chirp saved successfully!");
});

app.Run();

public record Chirp(string Author, string Message, long Timestamp);
