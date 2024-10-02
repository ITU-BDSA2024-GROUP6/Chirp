using System.Collections.Generic; 
using Xunit;

namespace unit;

public class UnitTests
{
    private readonly CheepService cheepService; 
    public UnitTests() 
    { 
        cheepService = new CheepService();
    }
    
    [Fact] 
    public void GetCheeps_ReturnsListOfCheeps() 
    {     
        // Given 
        var pageSize = 32;

        // When 
        var result = cheepService.GetCheeps(); 
        
        // Then 
        Assert.Equal(pageSize, result.Count);  
    }

    [Fact] 
    public void GetCheeps_ReturnsCorrectPageSize_WhenPageSizingIsUsed() 
    { 
        // Given 
        int page = 0;
        int pageSize = 10; 
        
        // When 
        var result = cheepService.GetCheeps(page, pageSize); 
        
        // Then 
        Assert.Equal(pageSize, result.Count);
    }

    [Fact]
    public void GetCheeps_ReturnsCorrectPage_WhenPagingIsUsed()
    {
        // Given
            int page_1 = 0;
            int page_2 = 1;

        // When
            var result_1 = cheepService.GetCheeps(page_1);
            var result_2 = cheepService.GetCheeps(page_2);

        // Then
            Assert.NotEqual(result_1, result_2);
    }

    [Fact]
    public void GetCheepsFromAuthor_OnlyReturnsCheepFromSpecificAuthor()
    {
        // Given
             var author = cheepService.GetCheeps(0,1)[0].Author;

        // When
            var result = cheepService.GetCheepsFromAuthor(author);

        // Then
            Assert.All(result, cheep => Assert.Equal(author, cheep.Author));
    }

    [Fact] 
    public void GetCheepsFromAuthor_ReturnsEmptyList_WhenAuthorDoesNotExist() 
    { 
        // Given 
        string nonExistentAuthor = "NonExistentAuthor"; 
        
        // When 
        var result = cheepService.GetCheepsFromAuthor(nonExistentAuthor); 
        
        // Then 
        Assert.Empty(result); 
    }

    [Fact] 
    public void GetCheeps_ReturnsCheepWithFormattedTimestamp() 
    { 
        // Given 
        var cheeps = cheepService.GetCheeps(0, 1); 
        
        // When 
        var firstCheep = cheeps[0]; 
        
        // Then 
        Assert.Matches(@"\d{2}/\d{2}/\d{2} \d{1,2}:\d{2}:\d{2}", firstCheep.Timestamp); 
    } 
}