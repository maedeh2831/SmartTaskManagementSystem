/*
| Module      : Collaboration
| Entity      : ChecklistItem
| Purpose     : پیکربندی موجودیت ChecklistItem.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class ChecklistItemConfiguration : IEntityTypeConfiguration<ChecklistItem>
    {
        public void Configure(EntityTypeBuilder<ChecklistItem> builder)
        {
            builder.ToTable("ChecklistItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(x => x.IsCompleted)
                .HasDefaultValue(false);

            builder.HasOne(x => x.Checklist)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.ChecklistId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.ChecklistId);
        }
    }
}