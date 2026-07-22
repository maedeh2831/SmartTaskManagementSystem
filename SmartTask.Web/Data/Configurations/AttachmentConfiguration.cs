/*
| Module      : Collaboration
| Entity      : Attachment
| Purpose     : پیکربندی موجودیت Attachment.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
    {
        public void Configure(EntityTypeBuilder<Attachment> builder)
        {
            builder.ToTable("Attachments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.FilePath)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.ContentType)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasOne(x => x.TaskItem)
                .WithMany(x => x.Attachments)
                .HasForeignKey(x => x.TaskItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ApplicationUser)
                .WithMany(x => x.Attachments)
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.TaskItemId);

            builder.HasIndex(x => x.ApplicationUserId);
        }
    }
}