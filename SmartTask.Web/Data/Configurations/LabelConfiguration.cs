/*
| Module      : Collaboration
| Entity      : Label
| Purpose     : پیکربندی موجودیت Label.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class LabelConfiguration : IEntityTypeConfiguration<Label>
    {
        public void Configure(EntityTypeBuilder<Label> builder)
        {
            builder.ToTable("Labels");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Color)
                .HasMaxLength(20);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.Labels)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.ProjectId);

            builder.HasIndex(x => new
            {
                x.ProjectId,
                x.Name
            }).IsUnique();
        }
    }
}