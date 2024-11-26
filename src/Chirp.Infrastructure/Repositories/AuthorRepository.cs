using Chirp.Core.Models;
using Chirp.Core.RepositoryInterfaces;
using Chirp.Infrastructure.Data;
using Chirp.Core.DTOs;
using System.Reflection.Metadata.Ecma335;

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
            return new AuthorDTO(){Name = author?.UserName?? "", Email = author?.Email?? ""};
        }
    }
}
