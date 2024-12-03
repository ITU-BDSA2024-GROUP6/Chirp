using Chirp.Core.DTOs;
using Chirp.Core.Models;

namespace Chirp.Core.RepositoryInterfaces
{
    public interface ICheepRepository
    {
        public List<CheepDTO> GetCheeps(int page, int pageSize);
        public List<CheepDTO> GetCheepsFromAuthor(Author author, int page, int pageSize); 
        public Task CreateCheep(string text, Author author, DateTime timestamp);
        public List<CheepDTO> GetUsersFollowingCheeps(Author author, int page, int pageSize);
    }
}