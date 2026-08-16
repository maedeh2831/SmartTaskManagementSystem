/*
| Module      : Database
| Entity      : ChatMessageConfiguration
| Purpose     : تنظیمات دیتابیس موجودیت ChatMessage.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.ToTable("ChatMessages");

            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.Content)
                   .HasMaxLength(4000);

            builder.Property(x => x.AttachmentPath)
                   .HasMaxLength(500);

            builder.Property(x => x.AttachmentName)
                   .HasMaxLength(260);

            // Indexes
            builder.HasIndex(x => new { x.ProjectId, x.Id });

            builder.HasIndex(x => x.SenderId);

            // Relationships
            builder.HasOne(x => x.Project)
                   .WithMany()
                   .HasForeignKey(x => x.ProjectId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Sender)
                   .WithMany()
                   .HasForeignKey(x => x.SenderId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReplyToMessage)
                   .WithMany(x => x.Replies)
                   .HasForeignKey(x => x.ReplyToMessageId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
