using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class ProjectSimulationConfiguration : IEntityTypeConfiguration<ProjectSimulation>
    {
        public void Configure(EntityTypeBuilder<ProjectSimulation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.CriticalPathLengthDays)
                .HasDefaultValue(0);

            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Scenarios)
                .WithOne(s => s.ProjectSimulation)
                .HasForeignKey(s => s.ProjectSimulationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            builder.HasIndex(x => x.ProjectId);
            builder.HasIndex(x => x.CreatedDate);
            builder.HasIndex(x => new { x.ProjectId, x.CreatedDate });
        }
    }
}
