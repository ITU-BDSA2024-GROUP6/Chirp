using MyChat.Razor.Exceptions;

namespace MyChat.Razor.Repositories
{
    public class CheepRepository : ICheepRepository
    {
        private readonly ChatDBContext _context;
        private readonly IAuthorRepository _authorRepository;

        public CheepRepository(ChatDBContext context, IAuthorRepository authorRepository)
        {
            _context = context;
            _authorRepository = authorRepository;
        }

        public List<CheepDTO> GetCheeps(int page, int pageSize)
        {
            return _context.Cheeps
                .Include(c => c.Author)
                .OrderBy(c => c.TimeStamp) 
                .Skip((page) * pageSize) 
                .Take(pageSize) 
                .Select(c => new CheepDTO 
                {
                    Text = c.Text, 
                    TimeStamp = c.TimeStamp.ToString(),
                    Author = new AuthorDTO 
                    {
                        Name = c.Author.Name,
                        Email = c.Author.Email
                    }
                })
                .ToList();
        }

        public List<CheepDTO> GetCheepsFromAuthor(string author, int page, int pageSize)
        {
            return _context.Cheeps
                .Include(c => c.Author)
                .OrderBy(c => c.TimeStamp)
                .Where(c => c.Author.Name == author) 
                .Skip((page) * pageSize)
                .Take(pageSize)
                .Select(c => new CheepDTO 
                {
                    Text = c.Text,
                    TimeStamp = c.TimeStamp.ToString(),
                    Author = new AuthorDTO
                    {
                        Name = c.Author.Name,
                        Email = c.Author.Email
                    }
                })
                .ToList();
        }

        public void CreateCheep(string text, string authorName, string authorEmail) 
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Cheep text cannot be empty.", nameof(text));
            if (string.IsNullOrWhiteSpace(authorName))
                throw new ArgumentException("Author name cannot be empty.", nameof(authorName));
            if (string.IsNullOrWhiteSpace(authorEmail))
                throw new ArgumentException("Author email cannot be empty.", nameof(authorEmail));

            Author? author = _authorRepository.GetAuthorByEmail(authorEmail);

            if (author == null)
            {
                _authorRepository.CreateAuthor(authorName, authorEmail);
                author = _authorRepository.GetAuthorByEmail(authorEmail);
                
                if (author == null)
                    throw new InvalidOperationException("Failed to create or retrieve author.");
            }

            var maxCheepId = _context.Cheeps.Any() ? _context.Cheeps.Max(cheep => cheep.CheepId) : 0;

            var newCheep = new Cheep
            {
                CheepId = maxCheepId + 1,
                AuthorId = author.AuthorId,
                Author = author,
                Text = text,
                TimeStamp = DateTime.Now
            };

            _context.Cheeps.Add(newCheep);
            _context.SaveChanges();
        }
    }
}