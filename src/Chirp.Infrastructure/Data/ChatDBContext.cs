using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Chirp.Core.Models;
using Chirp.Core.DTOs;

namespace Chirp.Infrastructure.Data
{  
    public class ChatDBContext : IdentityDbContext<Author>
    {
        public ChatDBContext(DbContextOptions<ChatDBContext> options) : base(options) {}
        public required DbSet<Author> Authors { get; set; }
        public required DbSet<Cheep> Cheeps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AuthorDTO>()
                .HasNoKey();

            modelBuilder.Entity<Author>()
                .HasMany(a => a.Following)
                .WithMany(a => a.Followers)
                .UsingEntity(j => j.ToTable("AuthorFollows"));
        }
    }
}