using Chirp.Core.RepositoryInterfaces;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Data;
using Chirp.Core.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
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
            _authorRepository = new AuthorRepository(_context);
            _cheepRepository = new CheepRepository(_context, _authorRepository);
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
                Id = Guid.NewGuid().ToString(),
                UserName = "John Doe",
                Email = "john@email.dk"
            };

            var author2 = new Author
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "Joe Smith",
                Email = "joe@email.dk"
            };

            _context.Authors.AddRange(author1, author2);
            await _context.SaveChangesAsync();

            // Follow Author
            var author1FromDb = _authorRepository.GetAuthorByName(author1.UserName);
            var author2FromDb = _authorRepository.GetAuthorByName(author2.UserName);

            // Null checks
            Assert.IsNotNull(author1FromDb, "Author1 not found in database");
            Assert.IsNotNull(author2FromDb, "Author2 not found in database");

            await _authorRepository.FollowAuthor(author1FromDb.UserName, author2FromDb.UserName);

            // Create Cheeps
            await _cheepRepository.CreateCheep("Hello World", author1FromDb, DateTime.Now);
            await _cheepRepository.CreateCheep("Another Cheep", author2FromDb, DateTime.Now);

            // Act
            var cheepsFromFollowers = _cheepRepository.GetUsersFollowingCheeps(author1FromDb, 0, 10);

            // Assert
            Assert.IsNotNull(cheepsFromFollowers, "Cheeps from followers should not be null");
            Assert.That(cheepsFromFollowers.Count(), Is.EqualTo(1), "Should return 1 cheep");
            Assert.That(cheepsFromFollowers.First().Text, Is.EqualTo("Another Cheep"), "The text of the cheep is incorrect");
        }
    }
}
