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
    [Table("Fintech_Transfer")]
    public class FintechTransfer
    {
        public Guid Id { get; set; }

        public Guid FromWalletId { get; set; }

        public Guid ToWalletId { get; set; }

        public decimal Amount { get; set; }

        public string? Description { get; set; }
    }

    public class FintechTransferConfiguration : IEntityTypeConfiguration<FintechTransfer>
    {
        public void Configure(EntityTypeBuilder<FintechTransfer> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).HasColumnType("uniqueidentifier");
            builder.Property(a => a.FromWalletId).HasColumnType("uniqueidentifier").IsRequired();
            builder.Property(a => a.ToWalletId).HasColumnType("uniqueidentifier").IsRequired();
            builder.Property(a => a.Amount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(a => a.Description).HasColumnType("nvarchar(255)");
        }
    }
}