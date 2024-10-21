namespace MyChat.Razor.Repositories
{
    public interface IAuthorRepository
    {
        public Author? getAuthorByName(string name);
        public Author? getAuthorByEmail(string email);
        public Author? getAuthorByID(int id);
        public bool createAuthor(string name, string email);  
    }

}