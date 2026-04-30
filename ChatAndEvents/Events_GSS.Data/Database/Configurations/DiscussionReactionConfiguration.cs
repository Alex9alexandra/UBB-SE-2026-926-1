using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.Database.Configurations;

public class DiscussionReactionConfiguration : IEntityTypeConfiguration<DiscussionReaction>
{
    public void Configure(EntityTypeBuilder<DiscussionReaction> e)
    {
        e.HasKey(dr => new { dr.DiscussionId, dr.UserId });

        e.HasOne(dr => dr.Discussion)
            .WithMany(d => d.Reactions)
            .HasForeignKey(dr => dr.DiscussionId);

        e.HasOne(dr => dr.User)
            .WithMany()
            .HasForeignKey(dr => dr.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}