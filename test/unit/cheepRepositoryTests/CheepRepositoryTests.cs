using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Moq;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Data;
using Chirp.Core.RepositoryInterfaces;
using Chirp.Core.Models;
using System;

namespace UnitTests
{
    [Category("Unit")]
    public class CheepRepositoryTests
    {
        private ChatDBContext _context = null!;
        private IAuthorRepository _authorRepository = null!;
        private ICheepRepository _cheepRepository = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ChatDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ChatDBContext(options);
            _authorRepository = new AuthorRepository(_context);
            _cheepRepository = new CheepRepository(_context, _authorRepository);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SeedDatabase()
        {
            var authors = new List<Author>
            {
                new Author { UserName = "TestAuthor1", Email = "author1@test.com" },
                new Author { UserName = "TestAuthor2", Email = "author2@test.com" },
                new Author { UserName = "TestAuthor3", Email = "author3@test.com" },
                new Author { UserName = "TestAuthor4", Email = "author4@test.com" }
            };

            _context.Authors.AddRange(authors);
            _context.SaveChanges();

            var cheeps = new List<Cheep>();

            foreach (var author in authors)
            {
                for (int i = 1; i <= 4; i++)
                {
                    cheeps.Add(new Cheep
                    {
                        Author = author,
                        Text = $"Test Cheep {i} from {author.UserName}",
                        TimeStamp = DateTime.UtcNow
                    });
                }
            }

            _context.Cheeps.AddRange(cheeps);
            _context.SaveChanges();
        }

        [Test]
        public void CreateCheep_CreatesCheep()
        {
            // Arrange
            var authorName = "TestAuthor1";
            var expectedAuthor = new Author
            {
                UserName = authorName
            };

            _context.Authors.Add(expectedAuthor);
            _context.SaveChanges();

            // Act
            var text = "Test Cheep";
            var dateTime = DateTime.UtcNow;

            var cheep = _cheepRepository.CreateCheep(text, expectedAuthor, dateTime);

            // Assert
            var retrievedCheep = _context.Cheeps.FirstOrDefault(c => c.Text == text && c.Author.UserName == authorName && c.TimeStamp == dateTime);

            Assert.IsNotNull(retrievedCheep, "The cheep should exist in the context after being created.");
            Assert.That(retrievedCheep.Text, Is.EqualTo(text), "The text of the cheep should match.");
            Assert.That(retrievedCheep.Author.UserName, Is.EqualTo(authorName), "The author name should match.");
            Assert.That(retrievedCheep.TimeStamp, Is.EqualTo(dateTime), "The creation time should match.");

        }

        [Test]
        public void GetCheeps_ReturnsListOfCheepDTOs()
        {
            // Arrange
            SeedDatabase();

            // Act
            var result = _cheepRepository.GetCheeps(0, 8);

            // Assert
            Assert.IsNotNull(result, "The list should not be null.");
            Assert.That(result.Count, Is.EqualTo(8), "The number of cheeps should be 8");
        }

        [Test]
        public void GetCheepsFromAuthor_ReturnsListOfCheepDTOsFromThatAuthor()
        {
            // Arrange
            SeedDatabase();
            var author = _context.Authors.SingleOrDefault(a => a.UserName == "TestAuthor1");

            Assert.IsNotNull(author, "Author should exist after seeding the database");

            // Act
            var result = _cheepRepository.GetCheepsFromAuthor(author, 0, 4);

            // Assert
            Assert.IsNotNull(result, "The list should not be null.");
            Assert.That(result.Count, Is.EqualTo(4), "The number of cheeps should be 4");
            Assert.IsTrue(result.All(c => c.AuthorDTO.Name == author.UserName), "All cheeps should belong to the specified author");
        }

        [Test]
        public void GetUsersFollowingCheeps_ReturnsListOfCheepDTOsFromThatUsersListOfFollowing()
        {
            // Arrange
            SeedDatabase();
            var author = _context.Authors.SingleOrDefault(a => a.UserName == "TestAuthor1");
            var followingAuthor = _context.Authors.SingleOrDefault(async => async.UserName == "TestAuthor2");

            Assert.IsNotNull(author, "Author should exist after seeding the database");
            Assert.IsNotNull(followingAuthor, "FollowingAuthor should exist after seeding the database");

            author.Following.Add(followingAuthor);

            // Act
            var result = _cheepRepository.GetUsersFollowingCheeps(author, 0, 4);

            // Assert
            Assert.IsNotNull(result, "The list should not be null.");
            Assert.That(result.Count, Is.EqualTo(4), "The number of cheeps should be 4");
            Assert.IsTrue(result.All(c => c.AuthorDTO.Name == followingAuthor.UserName), "All cheeps should belong to the specified author");
        }
    }
}