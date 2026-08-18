/*
| Module      : Database
| Entity      : SprintConfiguration
| Purpose     : تنظیمات دیتابیس موجودیت Sprint.
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

            // Properties
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Goal)
                .HasMaxLength(1000);

            builder.Property(x => x.StartDate)
                .IsRequired();

            builder.Property(x => x.EndDate)
                .IsRequired();

            builder.Property(x => x.Capacity)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            // Indexes
            builder.HasIndex(x => new { x.ProjectId, x.Name })
                .IsUnique()
                .HasFilter("[ViewState] = 1");

            // Relationships
            builder.HasOne(x => x.Project)
                .WithMany(x => x.Sprints)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}