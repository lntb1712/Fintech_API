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
    [Table("Fintech_Budget")]
    public class FintechBudget : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid WalletId { get; set; }

        public Guid CategoryId { get; set; }

        public decimal Amount { get; set; }

        public required string Period { get; set; } //MONTHLY | YEARLY

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

    }

    public class FintechBudgetConfiguration : IEntityTypeConfiguration<FintechBudget>
    {
        public void Configure(EntityTypeBuilder<FintechBudget> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).HasColumnType("uniqueidentifier");
            builder.Property(a => a.UserId).HasColumnType("uniqueidentifier").IsRequired();
            builder.Property(a => a.WalletId).HasColumnType("uniqueidentifier").IsRequired();
            builder.Property(a => a.CategoryId).HasColumnType("uniqueidentifier").IsRequired();
            builder.Property(a => a.Amount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(a => a.Period).HasColumnType("varchar(50)").IsRequired();
            builder.Property(a => a.StartDate).HasColumnType("datetime").IsRequired();
            builder.Property(a => a.EndDate).HasColumnType("datetime").IsRequired();
            builder.HasIndex(x => x.CreatedDate);
            builder.Property(x => x.CreatedDate).HasColumnType("datetime").IsRequired();
            builder.Property(x => x.UpdatedDate).HasColumnType("datetime");
            builder.Property(x => x.CreatedName).HasColumnType("nvarchar(50)");
            builder.Property(x => x.Updater).HasColumnType("nvarchar(50)");
        }
    }
}
