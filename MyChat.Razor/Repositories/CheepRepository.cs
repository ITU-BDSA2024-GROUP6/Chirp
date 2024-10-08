namespace MyChat.Razor.Repositories
{
    public class CheepRepository : ICheepRepository
    {
        private readonly ChatDBContext _context;

        public CheepRepository(ChatDBContext context)
        {
            _context = context;
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
    }
}