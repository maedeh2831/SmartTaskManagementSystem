/*
| Module      : Agile
| Entity      : UserStoryConfiguration
| Purpose     : پیکربندی موجودیت UserStory.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Data.Configurations
{
    public class UserStoryConfiguration : IEntityTypeConfiguration<UserStory>
    {
        public void Configure(EntityTypeBuilder<UserStory> builder)
        {
            builder.ToTable("UserStories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(x => x.Description)
                .HasMaxLength(3000);

            builder.Property(x => x.Priority)
                .HasDefaultValue(StoryPriorityType.Medium);

            builder.Property(x => x.Status)
                .HasDefaultValue(StoryStatusType.New);

            builder.Property(x => x.StoryPoint)
                .HasDefaultValue(0);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.UserStories)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Backlog)
                .WithMany(x => x.UserStories)
                .HasForeignKey(x => x.BacklogId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Sprint)
                .WithMany(x => x.UserStories)
                .HasForeignKey(x => x.SprintId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Owner)
                .WithMany()
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => x.OwnerId);

            builder.HasIndex(x => x.ProjectId);

            builder.HasIndex(x => x.BacklogId);

            builder.HasIndex(x => x.SprintId);

            builder.HasIndex(x => new
            {
                x.ProjectId,
                x.Status
            });
        }
    }
}