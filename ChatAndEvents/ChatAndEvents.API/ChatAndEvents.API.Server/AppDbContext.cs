using ChatModule.Models; // Your Chat namespace
using ChatModule.src.domain;
using Events_GSS.Data.Models; // Your Events namespace
using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.API.Server;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // --- Unified User (The bridge between both apps) ---
    public DbSet<User> Users { get; set; }

    // --- Chat Tables ---
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Participant> Participants { get; set; }
    public DbSet<Friend> Friends { get; set; }
    public DbSet<>

    // --- Events Tables ---
    public DbSet<Event> Events { get; set; }
    public DbSet<DiscussionMute> DiscussionMutes { get; set; }
    // Add the rest here: Achievements, Notifications, etc. [cite: 38]

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // As Technical Lead, you may need to define complex relationships here,
        [cite_start]// such as how a User can be both a 'MutedUser' and a 'MutedBy' user[cite: 50].
    }
}