namespace MyChat.Razor.Repositories
{
    public interface IAuthorRepository
    {
        public Author? GetAuthorByName(string name);
        public Author? GetAuthorByEmail(string email);
        public Author? GetAuthorByID(int id);
        public void CreateAuthor(string name, string email);  
    }

}