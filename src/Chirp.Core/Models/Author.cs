using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Chirp.Core.DTOs;
using Microsoft.AspNetCore.Identity;

namespace Chirp.Core.Models
{
    public class Author : IdentityUser
    {
        public override required string? UserName { get => base.UserName; set => base.UserName = value; }
        
        public ICollection<Cheep>? Cheeps { get; set; } = [];

        public virtual ICollection<Author> Following { get; set; } = new List<Author>();
        public virtual ICollection<Author> Followers { get; set; } = new List<Author>();

        [NotMapped]
        public int FollowersCount => Followers?.Count ?? 0;

        [NotMapped]
        public int FollowingCount => Following?.Count ?? 0;

    }
}