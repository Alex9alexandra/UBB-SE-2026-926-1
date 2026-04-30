using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.Database.Configurations;

public class AnnouncementReactionConfiguration : IEntityTypeConfiguration<AnnouncementReaction>
{
    public void Configure(EntityTypeBuilder<AnnouncementReaction> e)
    {
        e.HasKey(ar => new { ar.AnnouncementId, ar.UserId });

        e.HasOne(ar => ar.Announcement)
            .WithMany(a => a.Reactions)
            .HasForeignKey(ar => ar.AnnouncementId);

        e.HasOne(ar => ar.User)
            .WithMany()
            .HasForeignKey(ar => ar.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}