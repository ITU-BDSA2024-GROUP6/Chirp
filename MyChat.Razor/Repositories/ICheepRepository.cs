namespace MyChat.Razor.Repositories
{
    public interface ICheepRepository
    {
        public List<CheepDTO> GetCheeps(int page, int pageSize);
        public List<CheepDTO> GetCheepsFromAuthor(string author, int page, int pageSize); 
        public void createCheep(string text, string email, string name);
    }
}