using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.Database.Configurations;

public class QuestConfiguration : IEntityTypeConfiguration<Quest>
{
    public void Configure(EntityTypeBuilder<Quest> e)
    {
        e.HasKey(q => q.QuestId);

        e.Property(q => q.QuestId)
            .ValueGeneratedOnAdd();

        e.HasOne(q => q.Event)
            .WithMany(ev => ev.Quests)
            .HasForeignKey(q => q.EventId);
    }
}