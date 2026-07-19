/*
| Module      : Agile
| Entity      : SubTaskItemConfiguration
| Purpose     : پیکربندی موجودیت SubTaskItem.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class SubTaskItemConfiguration : IEntityTypeConfiguration<SubTaskItem>
    {
        public void Configure(EntityTypeBuilder<SubTaskItem> builder)
        {
            builder.ToTable("SubTaskItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(x => x.IsCompleted)
                .HasDefaultValue(false);

            builder.HasOne(x => x.TaskItem)
                .WithMany(x => x.SubTasks)
                .HasForeignKey(x => x.TaskItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.TaskItemId);
        }
    }
}