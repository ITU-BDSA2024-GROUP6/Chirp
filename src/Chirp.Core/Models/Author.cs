using Microsoft.AspNetCore.Identity;

namespace Chirp.Core.Models
{
    public class Author : IdentityUser
    {
        public ICollection<Cheep>? Cheeps { get; set; } = new List<Cheep>();
    }
}