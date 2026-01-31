using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Entities.Wallet
{
    [Table("Fintech_SharedWallet")]
    public class FintechSharedWallet : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid WalletId { get; set;}

        public Guid OwnerId { get; set; }
    }

    public class FintechSharedWalletConfiguration : IEntityTypeConfiguration<FintechSharedWallet>
    {
        public void Configure(EntityTypeBuilder<FintechSharedWallet> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).HasColumnType("uniqueidentifier");
            builder.Property(a => a.WalletId).HasColumnType("uniqueidentifier").IsRequired();
            builder.Property(a => a.OwnerId).HasColumnType("uniqueidentifier").IsRequired();
            builder.HasIndex(x => x.CreatedDate);
            builder.Property(x => x.CreatedDate).HasColumnType("datetime").IsRequired();
            builder.Property(x => x.UpdatedDate).HasColumnType("datetime");
            builder.Property(x => x.CreatedName).HasColumnType("nvarchar(50)");
            builder.Property(x => x.Updater).HasColumnType("nvarchar(50)");
        }
    }
}