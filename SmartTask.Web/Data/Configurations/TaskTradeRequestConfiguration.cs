using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations;

public class TaskTradeRequestConfiguration : IEntityTypeConfiguration<TaskTradeRequest>
{
    public void Configure(EntityTypeBuilder<TaskTradeRequest> builder)
    {
        builder.ToTable("TaskTradeRequests");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Message).HasMaxLength(500);

        builder.HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.RequesterUser)
            .WithMany()
            .HasForeignKey(x => x.RequesterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TargetUser)
            .WithMany()
            .HasForeignKey(x => x.TargetUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RequesterTask)
            .WithMany()
            .HasForeignKey(x => x.RequesterTaskId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TargetTask)
            .WithMany()
            .HasForeignKey(x => x.TargetTaskId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}