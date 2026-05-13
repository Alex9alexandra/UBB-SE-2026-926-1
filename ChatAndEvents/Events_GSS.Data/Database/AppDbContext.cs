using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.Database.Configurations;
using Events_GSS.Data.Models;
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
    public DbSet<MemoryLike> MemoryLikes { get; set; }
    public DbSet<UserReputationScore> UserReputationScores { get; set; }
    public DbSet<AnnouncementReadReceipt> AnnouncementReadReceipts { get; set; }
    public DbSet<QuestMemory> QuestMemories { get; set; }
    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<AnnouncementReaction> AnnouncementReactions { get; set; }
    
    public DbSet<Discussion> Discussions { get; set; }
    public DbSet<DiscussionMessage> DiscussionMessages { get; set; }
    public DbSet<DiscussionReaction> DiscussionReactions { get; set; }
    public DbSet<DiscussionMute> DiscussionMutes { get; set; }
    public DbSet<Quest> Quests { get; set; }
    
    public DbSet<Achievement> Achievements { get; set; }

    public DbSet<Notification> Notifications { get; set; }

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
        modelBuilder.ApplyConfiguration(new MemoryLikeConfiguration());
        modelBuilder.ApplyConfiguration(new UserReputationScoreConfiguration());
        modelBuilder.ApplyConfiguration(new AnnouncementReadReceiptConfiguration());
        modelBuilder.ApplyConfiguration(new QuestMemoryConfiguration());
        
        modelBuilder.ApplyConfiguration(new DiscussionConfiguration());
        modelBuilder.ApplyConfiguration(new DiscussionReactionConfiguration());
        modelBuilder.ApplyConfiguration(new DiscussionMuteConfiguration());
        modelBuilder.ApplyConfiguration(new QuestConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new AnnouncementConfiguration());
        modelBuilder.ApplyConfiguration(new AnnouncementReactionConfiguration());


        modelBuilder.Entity<Events_GSS.Data.Models.Category>().HasData(
        new Events_GSS.Data.Models.Category { CategoryId = 1, Title = "NATURE" },
        new Events_GSS.Data.Models.Category { CategoryId = 2, Title = "FITNESS" },
        new Events_GSS.Data.Models.Category { CategoryId = 3, Title = "MUSIC" },
        new Events_GSS.Data.Models.Category { CategoryId = 4, Title = "SOCIAL" },
        new Events_GSS.Data.Models.Category { CategoryId = 5, Title = "ART" },
        new Events_GSS.Data.Models.Category { CategoryId = 6, Title = "PETS" },
        new Events_GSS.Data.Models.Category { CategoryId = 7, Title = "TECH" },
        new Events_GSS.Data.Models.Category { CategoryId = 8, Title = "FUN" }
        );

        modelBuilder.Entity<Achievement>().HasData(
            new Achievement { AchievementId = 1, Name = "First Steps", Description = "Attend your first event.", IsUnlocked = false },
            new Achievement { AchievementId = 2, Name = "Proper Host", Description = "Create 3 events.", IsUnlocked = false },
            new Achievement { AchievementId = 3, Name = "Quest Solver", Description = "Approve 25 quest submissions.", IsUnlocked = false },
            new Achievement { AchievementId = 4, Name = "Memory Keeper", Description = "Add 50 memories with photos.", IsUnlocked = false },
            new Achievement { AchievementId = 5, Name = "Social Butterfly", Description = "Send 100 discussion messages.", IsUnlocked = false },
            new Achievement { AchievementId = 6, Name = "Event Veteran", Description = "Attend 10 events.", IsUnlocked = false },
            new Achievement { AchievementId = 7, Name = "Perfectionist", Description = "Complete every quest in an event.", IsUnlocked = false }
        );

    }
}
