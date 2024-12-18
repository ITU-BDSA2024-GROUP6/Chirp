using Chirp.Core.RepositoryInterfaces;
using Chirp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Data;
using Chirp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
 
namespace IntegrationTests
{
    [Category("Integration")]
    public class IntegrationTests : IDisposable
    {
        private ChatDBContext _context;
        private ICheepRepository _cheepRepository;
        private IAuthorRepository _authorRepository;
 
        public IntegrationTests()
        {
            // Setup in-memory SQLite database
            var options = new DbContextOptionsBuilder<ChatDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
 
            _context = new ChatDBContext(options);
 
            // Initialize repositories
            _cheepRepository = new CheepRepository(_context);
            _authorRepository = new AuthorRepository(_context);
        }
 
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
 
        [Test]
        public async Task CreateCheepsAndGetByFollowers_ReturnsCorrectCheeps()
        {
            // Arrange
            var author1 = new Author
            {
                UserName = "John Doe",
                Email = "john@email.dk",
                Cheeps = new List<Cheep>()
            };
 
            var author2 = new Author
            {
                UserName = "Joe Smith",
                Email = "joe@email.dk",
                Cheeps = new List<Cheep>()
            };
 
            _context.Authors.Add(author1);
            _context.Authors.Add(author2);
            await _context.SaveChangesAsync();
 
            // Follow Author
            var author1FromDb = _authorRepository.GetAuthorByName(author1.UserName);
            var author2FromDb = _authorRepository.GetAuthorByName(author2.UserName);
 
            await _authorRepository.FollowAuthor(author1FromDb.UserName, author2FromDb.UserName);
 
            // Create Cheeps
            var cheep1 = new Cheep
            {
                Id = "1",
                Text = "Hello World",
                TimeStamp = DateTime.Now,
                Author = author1FromDb
            };
 
            var cheep2 = new Cheep
            {
                Id = "2",
                Text = "Another Cheep",
                TimeStamp = DateTime.Now,
                Author = author2FromDb
            };
 
            await _cheepRepository.CreateCheep(cheep1);
            await _cheepRepository.CreateCheep(cheep2);
 
            // Act
            var followers = new List<string> { author1FromDb.Id };
            var cheepsFromFollowers = await _cheepRepository.GetByFollowers(followers, 0);
 
            // Assert
            Assert.IsNotNull(cheepsFromFollowers);
            Assert.That(cheepsFromFollowers.Count(), Is.EqualTo(1));
            Assert.That(cheepsFromFollowers.First().Text, Is.EqualTo("Another Cheep"));
        }
    }
}