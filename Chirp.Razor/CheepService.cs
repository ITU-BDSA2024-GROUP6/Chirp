using System.Collections.Generic;

public interface ICheepService
{
    public List<CheepViewModel> GetCheeps(int? page = 0, int? pageSize = 32);
    public List<CheepViewModel> GetCheepsFromAuthor(string author, int? page = 0, int? pageSize = 32);
}

public class CheepService : ICheepService
{
    private readonly DBFacade _dbFacade = new DBFacade();

    public List<CheepViewModel> GetCheeps(int? page = 0, int? pageSize = 32)
    {
        return _dbFacade.GetCheeps(page, pageSize);
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author, int? page = 0, int? pageSize = 32)
    {
        return _dbFacade.GetCheepsFromAuthor(author, page, pageSize);
    }
}