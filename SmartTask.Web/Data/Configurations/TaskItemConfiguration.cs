/*
| Module      : Agile
| Entity      : TaskItemConfiguration
| Purpose     : پیکربندی موجودیت TaskItem.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Data.Configurations
{
    public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
    {
        public void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            builder.ToTable("TaskItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(x => x.Description)
                .HasMaxLength(4000);

            builder.Property(x => x.Status)
                .HasDefaultValue(TaskStatusType.ToDo);

            builder.Property(x => x.Priority)
                .HasDefaultValue(TaskPriorityType.Medium);

            builder.Property(x => x.Type)
                .HasDefaultValue(TaskType.Task);

            builder.Property(x => x.Estimate)
                .HasDefaultValue(0);

            builder.HasOne(x => x.UserStory)
                .WithMany(x => x.Tasks)
                .HasForeignKey(x => x.UserStoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.UserStoryId);

            builder.HasIndex(x => new
            {
                x.UserStoryId,
                x.Status
            });

            builder.HasIndex(x => new
            {
                x.UserStoryId,
                x.Priority
            });
        }
    }
}