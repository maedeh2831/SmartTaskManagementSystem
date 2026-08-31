using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations;

public class AiDecisionLogConfiguration : IEntityTypeConfiguration<AiDecisionLog>
{
    public void Configure(EntityTypeBuilder<AiDecisionLog> builder)
    {
        builder.ToTable("AiDecisionLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.DecisionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.UserDecision)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.AiSuggestion)
            .HasMaxLength(2000);

        builder.Property(x => x.AiReasons)
            .HasMaxLength(4000);

        builder.Property(x => x.UserReason)
            .HasMaxLength(1000);

        builder.HasOne<SmartTask.Web.Models.Entities.ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
