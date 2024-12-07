using Microsoft.EntityFrameworkCore;
using Chirp.Core.Models;
using Chirp.Core.DTOs;
using Chirp.Core.RepositoryInterfaces;
using Chirp.Infrastructure.Data;

namespace Chirp.Infrastructure.Repositories
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
                .OrderByDescending(c => c.TimeStamp) 
                .Skip((page) * pageSize) 
                .Take(pageSize) 
                .Select(c => new CheepDTO 
                {
                    Text = c.Text, 
                    TimeStamp = c.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    AuthorDTO = _authorRepository.CreateAuthorDTO(c.Author)
                }).ToList();
        }

        public List<CheepDTO> GetCheepsFromAuthor(Author author, int page, int pageSize)
        {
            AuthorDTO authorDTO = _authorRepository.CreateAuthorDTO(author);
            return _context.Cheeps
                .Include(c => c.Author)
                .OrderByDescending(c => c.TimeStamp)
                .Where(c => c.Author == author) 
                .Skip(page * pageSize)
                .Take(pageSize)
                .Select(c => new CheepDTO 
                {
                    Text = c.Text,
                    TimeStamp = c.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    AuthorDTO = authorDTO
                }).ToList();
        }

        public List<CheepDTO> GetUsersFollowingCheeps(Author author, int page, int pageSize)
        {
            var _cheeps = new List<CheepDTO>();

            foreach (Author followingAuthor in author.Following)
            {
                _cheeps.AddRange
                (
                    _context.Cheeps
                .Include(c => c.Author)
                .OrderByDescending(c => c.TimeStamp)
                .Where(c => c.Author == followingAuthor) 
                .Skip(page * pageSize)
                .Take(pageSize)
                .Select(c => new CheepDTO 
                {
                    Text = c.Text,
                    TimeStamp = c.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    AuthorDTO = _authorRepository.CreateAuthorDTO(followingAuthor)
                }).ToList()
                );
            }

            return _cheeps;
        }



        public async Task CreateCheep(string text, Author author, DateTime dateTime) 
        {
            var result = _context.Authors.SingleOrDefault(a => a.UserName == author.UserName);

            if (result != null)
            {
                Cheep cheep = new (){ Author = result, Text = text, TimeStamp = dateTime};
                await _context.Cheeps.AddAsync(cheep);
            }

            await _context.SaveChangesAsync();
            }

    }
}