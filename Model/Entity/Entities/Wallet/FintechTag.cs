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
    [Table("Fintech_Tag")]
    public class FintechTag : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string? Name { get; set; }

        public string? Color { get; set; }
    }

    public class FintechTagConfiguration : IEntityTypeConfiguration<FintechTag>
    {
        public void Configure(EntityTypeBuilder<FintechTag> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).HasColumnType("uniqueidentifier");
            builder.Property(a => a.UserId).HasColumnType("uniqueidentifier").IsRequired();
            builder.Property(a => a.Name).HasColumnType("nvarchar(100)");
            builder.Property(a => a.Color).HasColumnType("nvarchar(20)");
            builder.HasIndex(x => x.CreatedDate);
            builder.Property(x => x.CreatedDate).HasColumnType("datetime").IsRequired();
            builder.Property(x => x.UpdatedDate).HasColumnType("datetime");
            builder.Property(x => x.CreatedName).HasColumnType("nvarchar(50)");
            builder.Property(x => x.Updater).HasColumnType("nvarchar(50)");
        }
    }
}
