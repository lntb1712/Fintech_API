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
    [Table("Fintech_Goal")]
    public class FintechGoal : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public required string Name { get; set; }

        public decimal TargetAmount { get; set; }

        public decimal CurrentAmount { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime TargetDate { get; set; }

        public int Status { get; set; }

        public string? Description { get; set; }

    }

    public class FintechGoalConfiguration : IEntityTypeConfiguration<FintechGoal>
    {
        public void Configure(EntityTypeBuilder<FintechGoal> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).HasColumnType("uniqueidentifier");
            builder.Property(a => a.UserId).HasColumnType("uniqueidentifier").IsRequired();
            builder.Property(a => a.Name).HasColumnType("nvarchar(255)").IsRequired();
            builder.Property(a => a.TargetAmount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(a => a.CurrentAmount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(a => a.StartDate).HasColumnType("datetime").IsRequired();
            builder.Property(a => a.TargetDate).HasColumnType("datetime").IsRequired();
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
