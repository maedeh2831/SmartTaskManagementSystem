/*
| Module      : Data Configuration
| Class       : MarketplaceTransactionConfiguration
| Purpose     : تنظیم نقشه‌برداری MarketplaceTransaction
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class MarketplaceTransactionConfiguration : IEntityTypeConfiguration<MarketplaceTransaction>
    {
        public void Configure(EntityTypeBuilder<MarketplaceTransaction> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.UserWalletId)
                .IsRequired();

            builder.Property(x => x.MarketplaceItemId)
                .IsRequired();

            builder.Property(x => x.PointsSpent)
                .IsRequired();

            builder.Property(x => x.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasDefaultValue(Models.Entities.TransactionStatus.Completed);

            builder.Property(x => x.TransactionDate)
                .IsRequired();

            // Indexes
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.UserWalletId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.TransactionDate);
            builder.HasIndex(x => new { x.UserId, x.TransactionDate });

            // Navigation
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.UserWallet)
                .WithMany()
                .HasForeignKey(x => x.UserWalletId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.MarketplaceItem)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.MarketplaceItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
