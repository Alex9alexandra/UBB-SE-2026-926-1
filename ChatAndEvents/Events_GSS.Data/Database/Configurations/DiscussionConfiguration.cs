using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.Database.Configurations;

public class DiscussionConfiguration : IEntityTypeConfiguration<Discussion>
{
    public void Configure(EntityTypeBuilder<Discussion> e)
    {
        e.HasKey(d => d.DiscussionId);

        e.Property(d => d.DiscussionId)
            .ValueGeneratedOnAdd();

        e.HasOne(d => d.Event)
            .WithMany(ev => ev.Discussions)
            .HasForeignKey(d => d.EventId);

        e.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}