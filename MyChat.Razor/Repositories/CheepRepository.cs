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

        public void createCheep(string text, string email, string name) 
        {
            var author = _authorRepository.getAuthorByEmail(email);

            if (author == null) 
            {
                Console.WriteLine("Author is null");
                var created = _authorRepository.createAuthor(name, email);
                Console.WriteLine("Author Created");
                if (!created)
                {
                    throw new Exception("Could not create author.");
                }

                author = _authorRepository.getAuthorByEmail(email);

                if (author == null)
                {
                    throw new Exception("Author creation failed unexpectedly.");
                }
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

            author.Cheeps.Add(newCheep);

            _context.Cheeps.AddRange(newCheep);
            _context.SaveChanges();
        }
    }
}