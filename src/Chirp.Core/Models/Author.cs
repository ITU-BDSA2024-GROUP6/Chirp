using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Chirp.Core.DTOs;
using Microsoft.AspNetCore.Identity;

namespace Chirp.Core.Models
{
    /// <summary>
    /// Represents an Author in the application, inheriting from IdentityUser.
    /// Authors can post Cheeps, follow other Authors, and be followed by others.
    /// </summary>
    public class Author : IdentityUser
    {
        public override required string? UserName { get => base.UserName; set => base.UserName = value; }
        
        public ICollection<Cheep>? Cheeps { get; set; } = [];

        public virtual ICollection<Author> Following { get; set; } = new List<Author>();

        public virtual ICollection<Author> Followers { get; set; } = new List<Author>();

        [NotMapped]
        // Dynamically calculates the number of followers without storing it in the database
        public int FollowersCount => Followers?.Count ?? 0;

        [NotMapped]
        // Dynamically calculates the number of authors the user is following without storing it in the database
        public int FollowingCount => Following?.Count ?? 0;
    }
}
