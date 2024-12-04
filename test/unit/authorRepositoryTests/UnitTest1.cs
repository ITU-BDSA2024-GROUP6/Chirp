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
    public class AuthorRepositoryTests : IDisposable
    {
        private ChatDBContext _context;
        private IAuthorRepository _authorRepository;

        public AuthorRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ChatDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ChatDBContext(options);

            _authorRepository = new AuthorRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public void GetAuthorByName_ReturnsCorrectAuthor()
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
            var result = _authorRepository.GetAuthorByName(authorName);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.That(result.UserName, Is.EqualTo(expectedAuthor.UserName));


        }

        [Test]
        public void GetAuthorByEmail_ReturnsCorrectAuthor()
        {
            // Arrange
            var authorName = "TestAuthor1";
            var authorEmail = "Test@Test.Test";
            var expectedAuthor = new Author
            {
                UserName = authorName,
                Email = authorEmail
            };

            _context.Authors.Add(expectedAuthor);
            _context.SaveChanges();

            // Act
            var result = _authorRepository.GetAuthorByEmail(authorEmail);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.That(result.Email, Is.EqualTo(expectedAuthor.Email));
        }

        [Test]
        public void GetAuthorById_ReturnsCorrectAuthor()
        {
            // Arrange
            var authorName = "TestAuthor1";
            var authorId = "1";
            var expectedAuthor = new Author
            {
                UserName = authorName,
                Id = authorId
            };

            _context.Authors.Add(expectedAuthor);
            _context.SaveChanges();

            // Act
            var result = _authorRepository.GetAuthorByID(authorId);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.That(result.Id, Is.EqualTo(expectedAuthor.Id));
        }

        [Test]
        public void createAuthorDTO_ReturnsCorrectAuthorDTO()
        {
            // Arrange
            var authorName = "TestAuthor1";
            var authorEmail = "Test@Test.Test";
            var expectedAuthor = new Author
            {
                UserName = authorName,
                Email = authorEmail
                
            };

            _context.Authors.Add(expectedAuthor);
            _context.SaveChanges();

            //Act 

            var result = _authorRepository.CreateAuthorDTO(expectedAuthor);

            //Assert

            Assert.IsNotNull(result, "Result should not be null");
            Assert.That(result.Name, Is.EqualTo(expectedAuthor.UserName));
            Assert.That(result.Email, Is.EqualTo(expectedAuthor.Email));
        }

        
    }
}
