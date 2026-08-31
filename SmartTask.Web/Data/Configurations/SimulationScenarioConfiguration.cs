using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class SimulationScenarioConfiguration : IEntityTypeConfiguration<SimulationScenario>
    {
        public void Configure(EntityTypeBuilder<SimulationScenario> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ScenarioName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.RiskLevel)
                .HasMaxLength(50)
                .HasDefaultValue("Medium");

            builder.Property(x => x.AffectedTasksJson)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.CriticalPathJson)
                .HasColumnType("nvarchar(max)");

            builder.HasOne(x => x.ProjectSimulation)
                .WithMany(s => s.Scenarios)
                .HasForeignKey(x => x.ProjectSimulationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            builder.HasIndex(x => x.ProjectSimulationId);
            builder.HasIndex(x => x.SimulatedTaskId);
            builder.HasIndex(x => x.SimulatedAt);
            builder.HasIndex(x => x.RiskLevel);
            builder.HasIndex(x => new { x.ProjectSimulationId, x.SimulatedAt });
        }
    }
}
