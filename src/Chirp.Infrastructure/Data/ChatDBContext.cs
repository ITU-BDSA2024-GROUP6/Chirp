using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Chirp.Core.Models;

namespace Chirp.Infrastructure.Data
{  
    public class ChatDBContext : IdentityDbContext
    {
        public ChatDBContext(DbContextOptions<ChatDBContext> options) : base(options) {}
        public DbSet<Author> Authors { get; set; }
        public DbSet<Cheep> Cheeps { get; set; }
    }
}