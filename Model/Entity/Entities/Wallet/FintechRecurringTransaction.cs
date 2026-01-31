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
    [Table("Fintech_RecurringTransaction")]
    public class FintechRecurringTransaction : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid WalletId { get; set; }

        public Guid CategoryId { get; set; }

        public decimal Amount { get; set; }

        public required string TransactionType { get; set; } // INCOME | EXPENSE    

        public required string Frequency { get; set; } // DAILY | WEEKLY | MONTHLY | YEARLY

        public int IntervalValue { get; set; } // e.g., every 2 weeks

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public DateTime NextRunDate { get; set; }

        public int Status { get; set; }

        public string? Description { get; set; }

    }

    public class FintechRecurringTransactionConfiguration : IEntityTypeConfiguration<FintechRecurringTransaction>
    {
        public void Configure(EntityTypeBuilder<FintechRecurringTransaction> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).HasColumnType("uniqueidentifier");
            builder.Property(a => a.UserId).HasColumnType("uniqueidentifier").IsRequired();
            builder.Property(a => a.WalletId).HasColumnType("uniqueidentifier").IsRequired();
            builder.Property(a => a.CategoryId).HasColumnType("uniqueidentifier").IsRequired();
            builder.Property(a => a.Amount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(a => a.TransactionType).HasColumnType("nvarchar(50)").IsRequired();
            builder.Property(a => a.Frequency).HasColumnType("nvarchar(50)").IsRequired();
            builder.Property(a => a.IntervalValue).HasColumnType("int").IsRequired();
            builder.Property(a => a.StartDate).HasColumnType("datetime").IsRequired();
            builder.Property(a => a.EndDate).HasColumnType("datetime");
            builder.Property(a => a.NextRunDate).HasColumnType("datetime").IsRequired();
            builder.Property(a => a.Status).HasColumnType("int").IsRequired();
            builder.Property(a => a.Description).HasColumnType("nvarchar(255)");
            builder.HasIndex(x => x.CreatedDate);
            builder.Property(x => x.CreatedDate).HasColumnType("datetime").IsRequired();
            builder.Property(x => x.UpdatedDate).HasColumnType("datetime");
            builder.Property(x => x.CreatedName).HasColumnType("nvarchar(50)");
            builder.Property(x => x.Updater).HasColumnType("nvarchar(50)");
        }
    }
}
