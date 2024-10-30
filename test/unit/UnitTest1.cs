using Microsoft.EntityFrameworkCore;
using MyChat.Razor.Repositories;
using Xunit;
using System;
using System.Linq;

namespace unit;

public class UnitTests : IDisposable
{
    private readonly ChatDBContext _context;
    private readonly IAuthorRepository _authorRepository;
    private readonly CheepRepository _cheepRepository;
        
    public UnitTests()
    {
        var options = new DbContextOptionsBuilder<ChatDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ChatDBContext(options);
        _authorRepository = new AuthorRepository(_context);
        _cheepRepository = new CheepRepository(_context, _authorRepository);
        
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        // Add some test data
        var author1 = new Author { 
            Name = "TestAuthor1", 
            Email = "test1@example.com",
            Cheeps = new List<Cheep>()  // List<T> implements ICollection<T>
        };
        var author2 = new Author { 
            Name = "TestAuthor2", 
            Email = "test2@example.com",
            Cheeps = new List<Cheep>()
        };
        _context.Authors.AddRange(author1, author2);
        
        var cheep1 = new Cheep { 
            Text = "Test Cheep 1", 
            Author = author1, 
            TimeStamp = DateTime.Now.AddHours(-1) 
        };
        var cheep2 = new Cheep { 
            Text = "Test Cheep 2", 
            Author = author2, 
            TimeStamp = DateTime.Now 
        };
        
        author1.Cheeps.Add(cheep1);
        author2.Cheeps.Add(cheep2);
        
        _context.Cheeps.AddRange(cheep1, cheep2);
        
        _context.SaveChanges();

        // Verify that data was actually added
        var cheepCount = _context.Cheeps.Count();
        if (cheepCount == 0)
        {
            throw new InvalidOperationException("Failed to seed the database. No cheeps were added.");
        }
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
        
    [Fact] 
    public void GetCheeps_ReturnsListOfCheeps() 
    {     
        // Given
        var pageSize = 32;

        // When 
        var result = _cheepRepository.GetCheeps(0, pageSize); 
        
        // Then 
        Assert.NotEmpty(result);
        Assert.True(result.Count <= pageSize, $"Expected at most {pageSize} cheeps, but got {result.Count}");
        
        // Additional debugging information
        if (result.Count == 0)
        {
            var dbCheepCount = _context.Cheeps.Count();
            throw new Exception($"No cheeps returned. Database contains {dbCheepCount} cheeps.");
        }
    }

    [Fact] 
    public void GetCheeps_ReturnsCorrectPageSize_WhenPageSizingIsUsed() 
    { 
        // Given 
        int page = 0;
        int pageSize = 1; 
        
        // When 
        var result = _cheepRepository.GetCheeps(page, pageSize); 
        
        // Then 
        Assert.Equal(pageSize, result.Count);
    }

    [Fact]
    public void GetCheeps_ReturnsCorrectPage_WhenPagingIsUsed()
    {
        // Given
        int page1 = 0;
        int page2 = 1;
        int pageSize = 1;

        // When
        var result1 = _cheepRepository.GetCheeps(page1, pageSize);
        var result2 = _cheepRepository.GetCheeps(page2, pageSize);

        // Then
        Assert.NotEqual(result1[0].Text, result2[0].Text);
    }

    [Fact]
    public void GetCheepsFromAuthor_OnlyReturnsCheepFromSpecificAuthorName()
    {
        // Given
        var authorName = "TestAuthor1";
        int page = 0;
        int pageSize = 32;

        // When
        var result = _cheepRepository.GetCheepsFromAuthor(authorName, page, pageSize);

        // Then
        Assert.All(result, cheepDTO => Assert.Equal(authorName, cheepDTO.Author.Name));
        Assert.NotEmpty(result);
    }

    [Fact] 
    public void GetCheepsFromAuthor_ReturnsEmptyList_WhenAuthorDoesNotExist() 
    { 
        // Given 
        string nonExistentAuthor = "NonExistentAuthor";
        int page = 0;
        int pageSize = 32; 
        
        // When 
        var result = _cheepRepository.GetCheepsFromAuthor(nonExistentAuthor, page, pageSize); 
        
        // Then 
        Assert.Empty(result); 
    }

    [Fact] 
    public void GetCheeps_ReturnsCheepWithValidTimestamp() 
    { 
        // Given 
        var cheeps = _cheepRepository.GetCheeps(0, 1); 
        
        // When 
        var firstCheep = cheeps.FirstOrDefault(); 
        
        // Then 
        Assert.NotNull(firstCheep);
        Assert.True(DateTime.TryParse(firstCheep.TimeStamp, out var parsedDate), 
            $"Could not parse the timestamp: {firstCheep.TimeStamp}");
        Assert.True(parsedDate <= DateTime.Now && parsedDate > DateTime.Now.AddDays(-1), 
            $"Timestamp {parsedDate} is not within the expected range");
    }

    [Fact]
    public void CreateAuthor_CreatesAnAuthor()
    {
        // Given
        string authorName = "NewTestName";
        string authorEmail = "newtest@author.email";

        // When
        _authorRepository.CreateAuthor(authorName, authorEmail);    

        // Then
        var result = _authorRepository.GetAuthorByName(authorName);
    
        Assert.NotNull(result);
        Assert.Equal(authorName, result?.Name); 
        Assert.Equal(authorEmail, result?.Email);
    }

    [Fact]
    public void CreateCheep_StoresCheepInDB_WhenAuthorDoesNotExist()
    {
        // Given
        string text = "New Test Cheep";
        string name = "NewTestAuthor";
        string email = "newtest@author.email";

        // When
        _cheepRepository.CreateCheep(text, name, email);

        var authorResult = _authorRepository.GetAuthorByName(name);
        var cheepResult = _cheepRepository.GetCheepsFromAuthor(name, 0, 32);

        // Then
        Assert.NotNull(authorResult);
        Assert.NotEmpty(cheepResult);
        Assert.Equal(text, cheepResult[0].Text);
    }

    [Fact]
    public void CreateCheep_ThrowsException_WhenInputIsInvalid()
    {
        // Given
        string text = "";
        string name = "TestAuthor";
        string email = "test@author.email";

        // When & Then
        Assert.Throws<ArgumentException>(() => _cheepRepository.CreateCheep(text, name, email));
    }
}