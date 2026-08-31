/*
| Module      : Data Configuration
| Class       : UserInventoryConfiguration
| Purpose     : تنظیم نقشه‌برداری UserInventory
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class UserInventoryConfiguration : IEntityTypeConfiguration<UserInventory>
    {
        public void Configure(EntityTypeBuilder<UserInventory> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.MarketplaceItemId)
                .IsRequired();

            builder.Property(x => x.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(x => x.IsEquipped)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.AcquiredDate)
                .IsRequired();

            // Indexes
            builder.HasIndex(x => new { x.UserId, x.MarketplaceItemId })
                .IsUnique();

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.IsEquipped);

            // Navigation
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.MarketplaceItem)
                .WithMany(x => x.UserInventories)
                .HasForeignKey(x => x.MarketplaceItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
