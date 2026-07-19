/*
| Module      : Agile
| Entity      : SprintConfiguration
| Purpose     : پیکربندی موجودیت Sprint.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class SprintConfiguration : IEntityTypeConfiguration<Sprint>
    {
        public void Configure(EntityTypeBuilder<Sprint> builder)
        {
            builder.ToTable("Sprints");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Goal)
                .HasMaxLength(1000);

            builder.Property(x => x.Status)
                .HasDefaultValue(Models.Enums.SprintStatusType.Planned);

            builder.Property(x => x.IsCompleted)
                .HasDefaultValue(false);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.Sprints)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.ProjectId);

            builder.HasIndex(x => new
            {
                x.ProjectId,
                x.Name
            }).IsUnique();
        }
    }
}