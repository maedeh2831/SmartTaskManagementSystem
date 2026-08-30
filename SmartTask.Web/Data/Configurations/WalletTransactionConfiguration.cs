/*
| Module      : Database
| Entity      : WalletTransactionConfiguration
| Purpose     : تنظیمات دیتابیس موجودیت WalletTransaction
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
    {
        public void Configure(EntityTypeBuilder<WalletTransaction> builder)
        {
            builder.ToTable("WalletTransactions");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.HasIndex(x => x.TransactionDate);
            builder.HasIndex(x => x.UserWalletId);

            builder.HasOne(x => x.UserWallet)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.UserWalletId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.UserProgression)
                .WithMany(x => x.WalletTransactions)
                .HasForeignKey(x => x.UserProgressionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
