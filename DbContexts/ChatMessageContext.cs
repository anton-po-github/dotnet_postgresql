using dotnet_postgresql.Entities;
using Microsoft.EntityFrameworkCore;

namespace dotnet_postgresql.DbContexts
{
    public class ChatMessageContext : DbContext
    {
        protected readonly IConfiguration _config;

        public ChatMessageContext(IConfiguration configuration)
        {
            _config = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(_config.GetConnectionString("ChatMessageConnection"));
        }

        public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
    }
}

