using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations;

public class SprintReportConfiguration : IEntityTypeConfiguration<SprintReport>
{
    public void Configure(EntityTypeBuilder<SprintReport> builder)
    {
        builder.ToTable("SprintReports");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.HasOne(x => x.Sprint)
            .WithMany()
            .HasForeignKey(x => x.SprintId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.GeneratedByUser)
            .WithMany()
            .HasForeignKey(x => x.GeneratedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}