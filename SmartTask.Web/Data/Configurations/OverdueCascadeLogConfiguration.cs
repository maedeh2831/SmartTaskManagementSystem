using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations;

public class OverdueCascadeLogConfiguration : IEntityTypeConfiguration<OverdueCascadeLog>
{
    public void Configure(EntityTypeBuilder<OverdueCascadeLog> builder)
    {
        builder.ToTable("OverdueCascadeLogs");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.SourceTask)
            .WithMany()
            .HasForeignKey(x => x.SourceTaskId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.ImpactedTask)
            .WithMany()
            .HasForeignKey(x => x.ImpactedTaskId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.SourceTaskId, x.ImpactedTaskId }).IsUnique();
    }
}