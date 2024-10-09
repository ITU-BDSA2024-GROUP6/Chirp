namespace MyChat.Razor.Repositories
{
    public interface IAuthorRepository
    {
        public Author getAuthorByName(string name);
        public Author getAuthorByEmail(string email);
        public bool createAuthor(string name, string email);  
    }

}