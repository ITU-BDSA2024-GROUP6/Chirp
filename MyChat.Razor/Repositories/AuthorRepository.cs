namespace MyChat.Razor.Repositories
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly ChatDBContext _context;

        public AuthorRepository(ChatDBContext context)
        {
            _context = context;
        }

        public Author getAuthorByName(string name)
        {
            return _context.Authors
                .FirstOrDefault(author => author.Name.ToLower() == name.ToLower());
        }

        public Author getAuthorByEmail(string email)
        {
            return _context.Authors
                .FirstOrDefault(author => author.Email.ToLower() == email.ToLower());
        }

        public Author getAuthorByID(int id)
        {
            return _context.Authors
                .FirstOrDefault(author => author.AuthorId == id);
        }

        public bool createAuthor(string name, string email)
        {
            // Check if the author already exists
            var existingAuthor = getAuthorByEmail(email);
            if (existingAuthor != null)
            {
                // Author already exists
                return false; // Or handle this case as needed
            }

            // Get the maximum AuthorId
            var maxAuthorId = _context.Authors.Any() ? _context.Authors.Max(a => a.AuthorId) : 0;

            // Create a new Author
            var newAuthor = new Author
            {
                AuthorId = maxAuthorId + 1,
                Name = name,
                Email = email,
                Cheeps = new List<Cheep>(),
            };

            // Add the new author to the context and save changes
            _context.Authors.Add(newAuthor);
            _context.SaveChanges(); // Save the changes to the database

            return true; // Successfully created author
        }
    }
}
