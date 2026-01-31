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
    [Table("Fintech_Wallet")]
    public class FintechWallet : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public required string Name { get; set; }

        public required string Type { get; set; } //CASH | EWALLET | BANK

        public required string Currency { get; set; } //USD | VND | EUR

        public required decimal Balance { get; set; }

        public int Status { get; set; }

    }

    public class FintechConfiguration : IEntityTypeConfiguration<FintechWallet>
    {
        public void Configure(EntityTypeBuilder<FintechWallet> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).HasColumnType("uniqueidentifier");
            builder.Property(a => a.UserId).HasColumnType("uniqueidentifier").IsRequired();
            builder.Property(a => a.Name).HasColumnType("nvarchar(100)").IsRequired();
            builder.Property(a => a.Type).HasColumnType("varchar(50)").IsRequired();
            builder.Property(a => a.Currency).HasColumnType("varchar(10)").IsRequired();
            builder.Property(a => a.Balance).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(a => a.Status).HasColumnType("int").IsRequired();
            builder.HasIndex(x => x.CreatedDate);
            builder.Property(x => x.CreatedDate).HasColumnType("datetime").IsRequired();
            builder.Property(x => x.UpdatedDate).HasColumnType("datetime");
            builder.Property(x => x.CreatedName).HasColumnType("nvarchar(50)");
            builder.Property(x => x.Updater).HasColumnType("nvarchar(50)");
        }
    }
}
