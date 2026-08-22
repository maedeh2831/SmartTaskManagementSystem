using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class ChatMessageReactionConfiguration : IEntityTypeConfiguration<ChatMessageReaction>
    {
        public void Configure(EntityTypeBuilder<ChatMessageReaction> builder)
        {
            builder.ToTable("ChatMessageReactions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Emoji)
                   .HasMaxLength(10);

            // Each user can only have one reaction per message
            builder.HasIndex(x => new { x.ChatMessageId, x.UserId })
                   .IsUnique();

            builder.HasIndex(x => x.ChatMessageId);

            builder.HasOne(x => x.ChatMessage)
                   .WithMany()
                   .HasForeignKey(x => x.ChatMessageId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
