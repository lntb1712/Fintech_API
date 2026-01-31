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
    [Table("Fintech_Transaction")]
    public class FintechTransaction : BaseEntity
    {
        public Guid Id { get; set; }
        
        public Guid UserId { get; set; }

        public Guid WalletId { get; set; }

        public Guid CategoryId { get; set; }

        public decimal Amount { get; set; } 

        public required string TransactionType { get; set; } // INCOME | EXPENSE | TRANSFER

        public string? Description { get; set; }   

        public required string Source { get; set; }  // MANUAL | SMS | IMPORT

        public string? ExternalReference{ get; set; }

    }

    public class FintechTransactionConfiguration : IEntityTypeConfiguration<FintechTransaction>
    {
        public void Configure(EntityTypeBuilder<FintechTransaction> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).HasColumnType("uniqueidentifier");
            builder.Property(a => a.UserId).HasColumnType("uniqueidentifier").IsRequired();
            builder.Property(a => a.WalletId).HasColumnType("uniqueidentifier").IsRequired();
            builder.Property(a => a.CategoryId).HasColumnType("uniqueidentifier").IsRequired();
            builder.Property(a => a.Amount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(a => a.TransactionType).HasColumnType("varchar(20)").IsRequired();
            builder.Property(a => a.Description).HasColumnType("nvarchar(255)");
            builder.Property(a => a.Source).HasColumnType("varchar(20)").IsRequired();
            builder.Property(a => a.ExternalReference).HasColumnType("nvarchar(100)");
            builder.HasIndex(x => x.CreatedDate);
            builder.Property(x => x.CreatedDate).HasColumnType("datetime").IsRequired();
            builder.Property(x => x.UpdatedDate).HasColumnType("datetime");
            builder.Property(x => x.CreatedName).HasColumnType("nvarchar(50)");
            builder.Property(x => x.Updater).HasColumnType("nvarchar(50)");
        }
    }
}
