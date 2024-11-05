using Chirp.Core.Models;
using Chirp.Core.DTOs;

namespace Chirp.Core.RepositoryInterfaces
{
    public interface IAuthorRepository
    {
        public Author? GetAuthorByName(string name);
        public Author? GetAuthorByEmail(string email);
        public Author? GetAuthorByID(string id);
        public Task<Author> CreateAuthor(AuthorDTO authorDto); 
    }

}