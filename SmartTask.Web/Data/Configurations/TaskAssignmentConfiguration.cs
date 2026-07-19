/*
| Module      : Agile
| Entity      : TaskAssignmentConfiguration
| Purpose     : پیکربندی موجودیت TaskAssignment.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class TaskAssignmentConfiguration : IEntityTypeConfiguration<TaskAssignment>
    {
        public void Configure(EntityTypeBuilder<TaskAssignment> builder)
        {
            builder.ToTable("TaskAssignments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AssignedDate)
                .HasDefaultValueSql("GETDATE()");

            builder.HasOne(x => x.TaskItem)
                .WithMany(x => x.Assignments)
                .HasForeignKey(x => x.TaskItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ApplicationUser)
                .WithMany(x => x.TaskAssignments)
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.TaskItemId);

            builder.HasIndex(x => x.ApplicationUserId);

            builder.HasIndex(x => new
            {
                x.TaskItemId,
                x.ApplicationUserId
            }).IsUnique();
        }
    }
}