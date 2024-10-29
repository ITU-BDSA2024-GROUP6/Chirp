using Microsoft.EntityFrameworkCore;
using Chirp.Core.Models;

namespace Chirp.Infrastructure.Data
{  
    public class ChatDBContext : DbContext
    {
        public ChatDBContext(DbContextOptions<ChatDBContext> options) : base(options) {}
        public DbSet<Author> Authors { get; set; }
        public DbSet<Cheep> Cheeps { get; set; }
    }
}