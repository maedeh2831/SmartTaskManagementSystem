/*
| Module      : Database
| Entity      : UserWalletConfiguration
| Purpose     : تنظیمات دیتابیس موجودیت UserWallet
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class UserWalletConfiguration : IEntityTypeConfiguration<UserWallet>
    {
        public void Configure(EntityTypeBuilder<UserWallet> builder)
        {
            builder.ToTable("UserWallets");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TotalPoints)
                .HasDefaultValue(0);
            builder.Property(x => x.AvailablePoints)
                .HasDefaultValue(0);
            builder.Property(x => x.SpentPoints)
                .HasDefaultValue(0);

            builder.HasIndex(x => x.UserId)
                .IsUnique();

            builder.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<UserWallet>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Transactions)
                .WithOne(x => x.UserWallet)
                .HasForeignKey(x => x.UserWalletId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
