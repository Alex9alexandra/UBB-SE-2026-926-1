using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.Database.Configurations;

public class UserReputationScoreConfiguration : IEntityTypeConfiguration<UserReputationScore>
{
    public void Configure(EntityTypeBuilder<UserReputationScore> e)
    {
        e.HasKey(r => r.UserId);

        e.HasOne(r => r.User)
            .WithOne(u => u.ReputationScore)
            .HasForeignKey<UserReputationScore>(r => r.UserId);

        e.ToTable("users_RP_scores");
    }
}