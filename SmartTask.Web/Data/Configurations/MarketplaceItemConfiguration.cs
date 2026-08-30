/*
| Module      : Data Configuration
| Class       : MarketplaceItemConfiguration
| Purpose     : تنظیم نقشه‌برداری MarketplaceItem
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class MarketplaceItemConfiguration : IEntityTypeConfiguration<MarketplaceItem>
    {
        public void Configure(EntityTypeBuilder<MarketplaceItem> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.Icon)
                .HasMaxLength(50);

            builder.Property(x => x.Color)
                .HasMaxLength(50);

            builder.Property(x => x.Category)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Price)
                .IsRequired();

            builder.Property(x => x.Stock)
                .IsRequired();

            builder.Property(x => x.TotalSold)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.IsLimitedTime)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            // Indexes
            builder.HasIndex(x => x.Category);
            builder.HasIndex(x => x.IsActive);
            builder.HasIndex(x => new { x.IsLimitedTime, x.AvailableFrom, x.AvailableUntil });

            // Navigation
            builder.HasMany(x => x.UserInventories)
                .WithOne(x => x.MarketplaceItem)
                .HasForeignKey(x => x.MarketplaceItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Transactions)
                .WithOne(x => x.MarketplaceItem)
                .HasForeignKey(x => x.MarketplaceItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
