/*
| Module      : Collaboration
| Entity      : TaskLabel
| Purpose     : پیکربندی موجودیت TaskLabel.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class TaskLabelConfiguration : IEntityTypeConfiguration<TaskLabel>
    {
        public void Configure(EntityTypeBuilder<TaskLabel> builder)
        {
            builder.ToTable("TaskLabels");

            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.TaskItem)
                .WithMany(x => x.TaskLabels)
                .HasForeignKey(x => x.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Label)
                .WithMany(x => x.TaskLabels)
                .HasForeignKey(x => x.LabelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.TaskItemId);

            builder.HasIndex(x => x.LabelId);

            builder.HasIndex(x => new
            {
                x.TaskItemId,
                x.LabelId
            }).IsUnique();
        }
    }
}