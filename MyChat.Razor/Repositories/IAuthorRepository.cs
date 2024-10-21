namespace MyChat.Razor.Repositories
{
    public interface IAuthorRepository
    {
        public Author? getAuthorByName(string name);
        public Author? getAuthorByEmail(string email);
        public Author? getAuthorByID(int id);
        public void createAuthor(string name, string email);  
    }

}