/*
| Module      : Agile
| Entity      : BacklogConfiguration
| Purpose     : پیکربندی موجودیت Backlog.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class BacklogConfiguration : IEntityTypeConfiguration<Backlog>
    {
        public void Configure(EntityTypeBuilder<Backlog> builder)
        {
            builder.ToTable("Backlogs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.HasOne(x => x.Project)
                .WithOne(x => x.Backlog)
                .HasForeignKey<Backlog>(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.ProjectId)
                .IsUnique();
        }
    }
}