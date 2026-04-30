using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.Database.Configurations;
using ChatAndEvents.Data.EventsData.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.Data.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Chat
    public DbSet<ChatData.domain.User> Users { get; set; }
    public DbSet<Friend> Friends { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Participant> Participants { get; set; }

    // Events
    public DbSet<Event> Events { get; set; }
    public DbSet<AttendedEvent> AttendedEvents { get; set; }
    public DbSet<Memory> Memories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new FriendConfiguration());
        modelBuilder.ApplyConfiguration(new ConversationConfiguration());
        modelBuilder.ApplyConfiguration(new MessageConfiguration());
        modelBuilder.ApplyConfiguration(new ParticipantConfiguration());

        modelBuilder.ApplyConfiguration(new EventConfiguration());
        modelBuilder.ApplyConfiguration(new AttendedEventConfiguration());
        modelBuilder.ApplyConfiguration(new MemoryConfiguration());
    }
}