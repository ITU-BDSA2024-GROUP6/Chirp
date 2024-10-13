
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.InMemory;
using MyChat.Razor.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace unit;

public class UnitTests
{
    private ChatDBContext _context = null!;
    private IAuthorRepository _authorRepository = null!;
    private CheepRepository _cheepRepository = null!;
        
    public UnitTests ()
    {
        var connectionString = Path.Combine(AppContext.BaseDirectory, "App_Data", "Chat.db");
        var options = new DbContextOptionsBuilder<ChatDBContext>()
            .UseSqlite($"Data Source={connectionString}")
            .Options;
        _context = new ChatDBContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
        _authorRepository = new AuthorRepository(_context);
        _cheepRepository = new CheepRepository(_context, _authorRepository);
    }
    
    [Fact] 
    public void GetCheeps_ReturnsListOfCheeps() 
    {     
        // Given
        var pageSize = 32;

        // When 
        var result = _cheepRepository.GetCheeps(1, pageSize); 
        
        // Then 
        Assert.True(result.Count <= pageSize);  
    }

    [Fact] 
    public void GetCheeps_ReturnsCorrectPageSize_WhenPageSizingIsUsed() 
    { 
        // Given 
        int page = 0;
        int pageSize = 10; 
        
        // When 
        var result = _cheepRepository.GetCheeps(page, pageSize); 
        
        // Then 
        Assert.True(result.Count <= pageSize);
    }

    [Fact]
    public void GetCheeps_ReturnsCorrectPage_WhenPagingIsUsed()
    {
        // Given
            int page_1 = 0;
            int page_2 = 1;
            int pageSize = 32;

        // When
            var result_1 = _cheepRepository.GetCheeps(page_1, pageSize);
            var result_2 = _cheepRepository.GetCheeps(page_2, pageSize);

        // Then
            Assert.NotEqual(result_1, result_2);
    }

    [Fact]
    public void GetCheepsFromAuthor_OnlyReturnsCheepFromSpecificAuthorName()
    {
        // Given
            var authorName = _cheepRepository.GetCheeps(0,1)[0].Author.Name;
            int page = 0;
            int pageSize = 32;

        // When
            var result = _cheepRepository.GetCheepsFromAuthor(authorName, page, pageSize);

        // Then
            Assert.All(result, cheepDTO => Assert.Equal(authorName, cheepDTO.Author.Name));
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
    public void GetCheeps_ReturnsCheepWithFormattedTimestamp() 
    { 
        // Given 
        var cheeps = _cheepRepository.GetCheeps(0, 1); 
        
        // When 
        var firstCheep = cheeps[0]; 

        Console.WriteLine(firstCheep.TimeStamp);
        
        // Then 
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}", firstCheep.TimeStamp); 
    } 

    [Fact]
    public void createAuthorCreatesAnAuthor()
    {
        // Given
        string authorName = "testName";
        string authorEmail = "testEmail@author.email";

        _authorRepository.createAuthor(authorName, authorEmail);    

    // Then
    var result = _authorRepository.getAuthorByName(authorName);
    
    Assert.NotNull(result);
    Assert.Equal(authorName, result.Name); 
    Assert.Equal(authorEmail, result.Email);
    }

    [Fact]
    public void createCheepStoresCheepInDBIfAuthorDoesNotExist()
    {
        // Given
        string text = @"Test";
        string name = "TestAuthor";
        string email = "Test@Author.Email";

        // When
        _cheepRepository.createCheep(text, name, email);

         var authorResult = _authorRepository.getAuthorByName(name);

        var cheepResult = _cheepRepository.GetCheepsFromAuthor(authorResult.Name, 0, 32);

        // Then
        Assert.NotNull(authorResult);  
    }
}