/*
| Module      : Database
| Entity      : ChatReadStateConfiguration
| Purpose     : تنظیمات دیتابیس موجودیت ChatReadState.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class ChatReadStateConfiguration : IEntityTypeConfiguration<ChatReadState>
    {
        public void Configure(EntityTypeBuilder<ChatReadState> builder)
        {
            builder.ToTable("ChatReadStates");

            builder.HasKey(x => x.Id);

            // Indexes
            builder.HasIndex(x => new { x.ProjectId, x.ApplicationUserId })
                   .IsUnique();

            // Relationships
            builder.HasOne(x => x.Project)
                   .WithMany()
                   .HasForeignKey(x => x.ProjectId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ApplicationUser)
                   .WithMany()
                   .HasForeignKey(x => x.ApplicationUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
