using Chirp.Core.Models;
using Chirp.Core.RepositoryInterfaces;
using Chirp.Infrastructure.Data;
using Chirp.Core.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Repositories
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly ChatDBContext _context;

        public AuthorRepository(ChatDBContext context)
        {
            _context = context;
        }

        public Author? GetAuthorByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be null or empty.", nameof(name));
            }

            return _context.Authors
                .Include(a => a.Following)
                .Include(a => a.Followers)
                .FirstOrDefault(author => author.UserName != null && author.UserName.ToLower() == name.ToLower());
        }

        public Author? GetAuthorByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            }

            return _context.Authors
                .FirstOrDefault(author => author.Email != null && author.Email.ToLower() == email.ToLower());
        }

        public Author? GetAuthorByID(string id)
        {
            return _context.Authors
                .FirstOrDefault(author => author.Id.Equals(id));
        }

        public AuthorDTO CreateAuthorDTO(Author author)
        {
            return new AuthorDTO() { Name = author?.UserName ?? "", Email = author?.Email ?? "" };
        }

        public async Task<bool> IsFollowing(string followerName, string followedName)
        {
            var follower = await _context.Authors
                .Include(a => a.Following)
                .FirstOrDefaultAsync(a => a.UserName != null && a.UserName.ToLower() == followerName.ToLower());

            if (follower == null) return false;

            return follower.Following.Any(f => f.UserName != null && f.UserName.ToLower() == followedName.ToLower());
        }

        public async Task FollowAuthor(string followerName, string followedName)
        {
            var follower = await _context.Authors
                .Include(a => a.Following)
                .FirstOrDefaultAsync(a => a.UserName != null && a.UserName.ToLower() == followerName.ToLower());

            var followed = await _context.Authors
                .FirstOrDefaultAsync(a => a.UserName != null && a.UserName.ToLower() == followedName.ToLower());

            if (follower == null || followed == null)
            {
                throw new ArgumentException("One or both users not found");
            }

            // Check if already following
            if (!await IsFollowing(followerName, followedName))
            {
                follower.Following.Add(followed);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UnfollowAuthor(string followerName, string followedName)
        {
            var follower = await _context.Authors
                .Include(a => a.Following)
                .FirstOrDefaultAsync(a => a.UserName != null && a.UserName.ToLower() == followerName.ToLower());

            var followed = await _context.Authors
                .FirstOrDefaultAsync(a => a.UserName != null && a.UserName.ToLower() == followedName.ToLower());

            if (follower == null || followed == null)
            {
                throw new ArgumentException("One or both users not found");
            }

            if (await IsFollowing(followerName, followedName))
            {
                follower.Following.Remove(followed);
                await _context.SaveChangesAsync();
            }
        }

        public List<AuthorDTO> GetFollowers(Author author)
        {
            return _context.Authors
                .Where(a => a.Id == author.Id) // Filter to find the specific author
                .SelectMany(a => a.Following) // Flatten the Following list
                .Select(f => new AuthorDTO
                {
                    Name = f.UserName,
                    Email = f.Email
                })
                .ToList();
        }
    }
}
