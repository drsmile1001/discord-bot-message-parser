using Microsoft.EntityFrameworkCore;

namespace DiscordBotMessageParser;

public class BotDbContext : DbContext
{
    public BotDbContext(DbContextOptions<BotDbContext> options) : base(options)
    {
    }

    public DbSet<MessagePreset> MessagePreset { get; set; } = null!;
}