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

        // Constructor to inject dependencies for database context and author repository
        public CheepRepository(ChatDBContext context, IAuthorRepository authorRepository)
        {
            _context = context;
            _authorRepository = authorRepository;
        }

        // Retrieves a paginated list of Cheeps, ordered by most recent first
        public List<CheepDTO> GetCheeps(int page, int pageSize)
        {
            return _context.Cheeps
                .Include(c => c.Author) // Includes author details for each Cheep
                .OrderByDescending(c => c.TimeStamp) // Orders by newest Cheeps
                .Skip(page * pageSize) // Skips Cheeps for previous pages
                .Take(pageSize) // Takes the specified number of Cheeps for the current page
                .Select(c => new CheepDTO 
                {
                    Text = c.Text, 
                    TimeStamp = c.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"), // Formats timestamp
                    AuthorDTO = _authorRepository.CreateAuthorDTO(c.Author) // Creates an authorDTO to display with the cheepDTO
                }).ToList();
        }

        // Retrieves a paginated list of Cheeps by a specific author
        public List<CheepDTO> GetCheepsFromAuthor(Author author, int page, int pageSize)
        {
            AuthorDTO authorDTO = _authorRepository.CreateAuthorDTO(author);
            return _context.Cheeps
                .Include(c => c.Author) // Includes author details
                .OrderByDescending(c => c.TimeStamp) // Orders by newest Cheeps
                .Where(c => c.Author == author) // Filters Cheeps by the given author
                .Skip(page * pageSize) // Skips Cheeps for previous pages
                .Take(pageSize) // Takes the specified number of Cheeps for the current page
                .Select(c => new CheepDTO 
                {
                    Text = c.Text,
                    TimeStamp = c.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"), // Formats timestamp
                    AuthorDTO = authorDTO // Uses pre-created AuthorDTO for optimization
                }).ToList();
        }

        // Retrieves a paginated list of Cheeps from authors that the given author is following
        public List<CheepDTO> GetUsersFollowingCheeps(Author author, int page, int pageSize)
        {
            var _cheeps = new List<CheepDTO>();

            foreach (Author followingAuthor in author.Following) // Iterates through followed authors
            {
                _cheeps.AddRange
                (
                    _context.Cheeps
                    .Include(c => c.Author) // Includes author details
                    .OrderByDescending(c => c.TimeStamp) // Orders by newest Cheeps
                    .Where(c => c.Author == followingAuthor) // Filters Cheeps by each followed author
                    .Skip(page * pageSize) // Skips Cheeps for previous pages
                    .Take(pageSize) // Takes the specified number of Cheeps for the current page
                    .Select(c => new CheepDTO 
                    {
                        Text = c.Text,
                        TimeStamp = c.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"), // Formats timestamp
                        AuthorDTO = _authorRepository.CreateAuthorDTO(followingAuthor) // Maps Author to AuthorDTO
                    }).ToList()
                );
            }

            return _cheeps;
        }

        // Creates a new Cheep with the specified text, author, and timestamp
        public async Task CreateCheep(string text, Author author, DateTime dateTime) 
        {
            var result = _context.Authors.SingleOrDefault(a => a.UserName == author.UserName); // Finds the author in the database

            if (result != null)
            {
                Cheep cheep = new (){ Author = result, Text = text, TimeStamp = dateTime }; // Creates a new Cheep instance
                await _context.Cheeps.AddAsync(cheep); // Adds the new Cheep to the database
            }

            await _context.SaveChangesAsync(); // Saves changes to the database
        }
    }
}
