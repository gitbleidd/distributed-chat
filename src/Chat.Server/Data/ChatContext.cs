using Chat.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chat.Server.Data
{
    public class ChatContext : DbContext
    {
        public DbSet<Message> Books => Set<Message>();

        public ChatContext(DbContextOptions<ChatContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSnakeCaseNamingConvention();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Constants.SchemaName);
            modelBuilder.Entity<Message>()
                .ToTable("message");
        }
    }
}
