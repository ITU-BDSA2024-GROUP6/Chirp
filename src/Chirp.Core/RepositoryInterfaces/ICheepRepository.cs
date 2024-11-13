using Chirp.Core.DTOs;
using Chirp.Core.Models;

namespace Chirp.Core.RepositoryInterfaces
{
    public interface ICheepRepository
    {
        public Author? GetAuthorByEmail(string name); // Add this method

        public List<CheepDTO> GetCheeps(int page, int pageSize);
        public List<CheepDTO> GetCheepsFromAuthor(string author, int page, int pageSize); 
        public Task CreateCheep(string text, Author author, DateTime timestamp);

    }
}