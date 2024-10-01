using Microsoft.Data.Sqlite; 
using System.Collections.Generic; 
using System.Linq;
using System.Data;

public interface ICheepService
{
    public List<CheepViewModel> GetCheeps();
    public List<CheepViewModel> GetCheepsFromAuthor(string author);
}

public class CheepService : ICheepService
{
    private readonly DBFacade _dbFacade = new DBFacade(); // Field initialized directly

    public List<CheepViewModel> GetCheeps()
    {
        return _dbFacade.GetCheeps();
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author)
    {
        return _dbFacade.GetCheepsFromAuthor(author);
    }
}
