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

        [Test]
        public async Task FollowAuthor_AddsTheAuthorToFollowersList()
        {
            // Arrange
            var authorName1 = "TestAuthor1";
            var expectedAuthor1 = new Author { UserName = authorName1 };

            var authorName2 = "TestAuthor2";
            var expectedAuthor2 = new Author { UserName = authorName2 };

            _context.Authors.Add(expectedAuthor1);
            _context.Authors.Add(expectedAuthor2);
            await _context.SaveChangesAsync();

            // Act
            await _authorRepository.FollowAuthor(expectedAuthor1.UserName, expectedAuthor2.UserName);

            var updatedAuthor2 = await _context.Authors
                .Include(a => a.Followers)
                .FirstOrDefaultAsync(a => a.UserName == authorName2);

            // Assert
            Assert.IsNotNull(updatedAuthor2);
            Assert.That(updatedAuthor2.Followers.Any(f => f.UserName == authorName1), Is.True);
        }

        [Test]
        public async Task UnfollowAuthor_RemovesTheAuthorFromFollowersList()
        {
            // Arrange
            var authorName1 = "TestAuthor1";
            var expectedAuthor1 = new Author { UserName = authorName1 };

            var authorName2 = "TestAuthor2";
            var expectedAuthor2 = new Author { UserName = authorName2 };

            _context.Authors.Add(expectedAuthor1);
            _context.Authors.Add(expectedAuthor2);
            await _context.SaveChangesAsync();

            expectedAuthor1.Followers.Add(expectedAuthor2);
            expectedAuthor2.Following.Add(expectedAuthor1);
            await _context.SaveChangesAsync();

            // Act
            await _authorRepository.UnfollowAuthor(expectedAuthor2.UserName, expectedAuthor1.UserName);

            var updatedAuthor1 = await _context.Authors
                .Include(a => a.Followers)
                .FirstOrDefaultAsync(a => a.UserName == authorName1);

            // Assert
            Assert.IsNotNull(updatedAuthor1);
            Assert.That(updatedAuthor1.Followers.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task IsFollowing_ReturnsTrue_WhenUserIsFollowingAnotherUser()
        {
            // Arrange
            var followerName = "FollowerUser";
            var followedName = "FollowedUser";

            var follower = new Author { UserName = followerName };
            var followed = new Author { UserName = followedName };

            _context.Authors.Add(follower);
            _context.Authors.Add(followed);
            await _context.SaveChangesAsync();

            // Establish the follow relationship
            follower.Following.Add(followed);
            await _context.SaveChangesAsync();

            // Act
            var result = await _authorRepository.IsFollowing(followerName, followedName);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task GetFollowers_ReturnsAuthorsThatTheAuthorIsFollowing()
        {
            // Arrange
            var targetAuthor = new Author { UserName = "TargetUser" };
            var followedAuthor1 = new Author { UserName = "FollowedUser1" };
            var unrelatedAuthor = new Author { UserName = "UnrelatedUser" };

            _context.Authors.Add(targetAuthor);
            _context.Authors.Add(followedAuthor1);
            _context.Authors.Add(unrelatedAuthor);
            await _context.SaveChangesAsync();

            targetAuthor.Following.Add(followedAuthor1);
            await _context.SaveChangesAsync();

            // Act
            var followers = _authorRepository.GetFollowers(targetAuthor);

            // Assert
            Assert.That(followers.Count, Is.EqualTo(1));
            Assert.That(followers.Any(f => f.Name == "FollowedUser1"), Is.True);
            Assert.That(followers.Any(f => f.Name == "UnrelatedUser"), Is.False);
        }
    }
}
