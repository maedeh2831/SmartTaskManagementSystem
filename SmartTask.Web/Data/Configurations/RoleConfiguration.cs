/*
| Module      : Database
| Entity      : RoleConfiguration
| Purpose     : تنظیمات دیتابیس موجودیت Role.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Description)
                .HasMaxLength(250);

            builder.HasIndex(x => x.Name)
                .IsUnique();
        }
    }
}