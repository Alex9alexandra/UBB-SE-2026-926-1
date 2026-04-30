using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.Database.Configurations;

public class DiscussionMuteConfiguration : IEntityTypeConfiguration<DiscussionMute>
{
    public void Configure(EntityTypeBuilder<DiscussionMute> e)
    {
        e.HasKey(dm => new { dm.DiscussionId, dm.UserId });

        e.HasOne(dm => dm.Discussion)
            .WithMany(d => d.Mutes)
            .HasForeignKey(dm => dm.DiscussionId);

        e.HasOne(dm => dm.User)
            .WithMany()
            .HasForeignKey(dm => dm.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}