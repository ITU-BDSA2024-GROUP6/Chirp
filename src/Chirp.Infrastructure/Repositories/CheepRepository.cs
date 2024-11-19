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
                    TimeStamp = c.TimeStamp.ToString(),
                    Author = new AuthorDTO 
                    {
                        Name = c.Author.UserName ?? "",
                        Email = c.Author.Email ?? ""
                    }
                })
                .ToList();
        }

        public Author? GetAuthorByEmail(string email)
        {
            return _authorRepository.GetAuthorByEmail(email); 
        }

        public Author? GetAuthorByName(string name)
        {
            return _authorRepository.GetAuthorByName(name); 
        }

        public List<CheepDTO> GetCheepsFromAuthor(string author, int page, int pageSize)
        {
            return _context.Cheeps
                .Include(c => c.Author)
                .OrderByDescending(c => c.TimeStamp)
                .Where(c => c.Author.UserName == author) 
                .Skip((page) * pageSize)
                .Take(pageSize)
                .Select(c => new CheepDTO 
                {
                    Text = c.Text,
                    TimeStamp = c.TimeStamp.ToString(),
                    Author = new AuthorDTO
                    {
                        Name = c.Author.UserName ?? "",
                        Email = c.Author.Email ?? ""
                    }
                })
                .ToList();
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