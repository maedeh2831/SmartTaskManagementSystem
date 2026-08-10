/*
| Module      : Database
| Entity      : ApplicationUserConfiguration
| Purpose     : تنظیمات دیتابیس موجودیت ApplicationUser.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("ApplicationUsers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.UserName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.PhoneNumber)
                .HasMaxLength(20);

            builder.Property(x => x.Avatar)
                .HasMaxLength(500);

            builder.Property(x => x.Bio)
                .HasMaxLength(1000);

            builder.Property(x => x.JobTitle)
                .HasMaxLength(100);

            builder.Property(x => x.TimeZone)
                .HasMaxLength(100);

            builder.HasIndex(x => x.Email)
                .IsUnique();

            builder.HasIndex(x => x.UserName)
                .IsUnique();

            builder.Property(x => x.CreatedDate)
                .HasDefaultValueSql("GETDATE()");

            builder.HasOne(u => u.DefaultWorkspace)
                   .WithMany()
                   .HasForeignKey(u => u.DefaultWorkspaceId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}