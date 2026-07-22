using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class TaskDependencyConfiguration : IEntityTypeConfiguration<TaskDependency>
    {
        public void Configure(EntityTypeBuilder<TaskDependency> builder)
        {
            builder.ToTable("TaskDependencies");

            builder.HasKey(td => td.Id);

            builder.Property(td => td.IsRequired)
                   .HasDefaultValue(true);

            // Task اصلی
            builder.HasOne(td => td.TaskItem)
                   .WithMany()
                   .HasForeignKey(td => td.TaskItemId)
                   .OnDelete(DeleteBehavior.NoAction);

            // Task وابسته
            builder.HasOne(td => td.DependsOnTaskItem)
                   .WithMany()
                   .HasForeignKey(td => td.DependsOnTaskItemId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(td => td.TaskItemId);

            builder.HasIndex(td => td.DependsOnTaskItemId);

            builder.HasIndex(td => new
            {
                td.TaskItemId,
                td.DependsOnTaskItemId
            }).IsUnique();
        }
    }
}