using Chirp.Core.Models;
using Chirp.Core.DTOs;

namespace Chirp.Core.RepositoryInterfaces
{
    public interface IAuthorRepository
    {
        public Author? GetAuthorByName(string name);
        public Author? GetAuthorByEmail(string email);
        public Author? GetAuthorByID(string id);
        public AuthorDTO CreateAuthorDTO(Author author) {return new AuthorDTO(){Name = author.UserName ?? "", Email = author.Email ?? ""};}

    }

}