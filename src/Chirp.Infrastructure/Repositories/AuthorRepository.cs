using MyChat.Razor.Exceptions;

namespace MyChat.Razor.Repositories
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly ChatDBContext _context;

        public AuthorRepository(ChatDBContext context)
        {
            _context = context;
        }

        public Author? GetAuthorByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be null or empty.", nameof(name));
            }

            return _context.Authors
                .FirstOrDefault(author => author.Name.ToLower() == name.ToLower());
        }

        public Author? GetAuthorByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            }

            return _context.Authors
                .FirstOrDefault(author => author.Email.ToLower() == email.ToLower());
        }

        public Author? GetAuthorByID(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Id must be a positive integer.", nameof(id));
            }

            return _context.Authors
                .FirstOrDefault(author => author.AuthorId == id);
        }

        public void CreateAuthor(string name, string email)
        {
            // Check if the author already exists
            if (_context.Authors.Any(a => a.Email.ToLower() == email.ToLower()))
            {
                throw new DuplicateAuthorException($"Author with email '{email}' already exists.");
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
        }
    }
}
