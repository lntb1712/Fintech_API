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
    [Table("Fintech_TransactionTag")]
    public class FintechTransactionTag : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid TagId { get; set; }

        public string Description { get; set; } = null!;

    }
    public class FintechTransactionTagConfiguration : IEntityTypeConfiguration<FintechTransactionTag>
    {
        public void Configure(EntityTypeBuilder<FintechTransactionTag> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).HasColumnType("uniqueidentifier");
            builder.Property(a => a.TagId).HasColumnType("uniqueidentifier").IsRequired();
            builder.Property(a => a.Description).HasColumnType("nvarchar(255)").IsRequired();
            builder.HasIndex(x => x.CreatedDate);
            builder.Property(x => x.CreatedDate).HasColumnType("datetime").IsRequired();
            builder.Property(x => x.UpdatedDate).HasColumnType("datetime");
            builder.Property(x => x.CreatedName).HasColumnType("nvarchar(50)");
            builder.Property(x => x.Updater).HasColumnType("nvarchar(50)");
        }
    }
}
