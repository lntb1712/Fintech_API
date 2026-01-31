using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Entities.Wallet
{
    [Table("Fintech_Category")]
    public class FintechCategory : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string? Name { get; set; }

        public Guid ParentId { get; set; }

        public required string Type { get; set; }

    }

    public class FintechCategoryConfiguration : IEntityTypeConfiguration<FintechCategory>
    {
        public void Configure(EntityTypeBuilder<FintechCategory> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).HasColumnType("uniqueidentifier");
            builder.Property(a => a.UserId).HasColumnType("uniqueidentifier").IsRequired();
            builder.Property(a => a.Name).HasColumnType("nvarchar(255)");
            builder.Property(a => a.ParentId).HasColumnType("uniqueidentifier").IsRequired();
            builder.Property(a => a.Type).HasColumnType("varchar(50)").IsRequired();
            builder.HasIndex(x => x.CreatedDate);
            builder.Property(x => x.CreatedDate).HasColumnType("datetime").IsRequired();
            builder.Property(x => x.UpdatedDate).HasColumnType("datetime");
            builder.Property(x => x.CreatedName).HasColumnType("nvarchar(50)");
            builder.Property(x => x.Updater).HasColumnType("nvarchar(50)");
        }
    }
}
