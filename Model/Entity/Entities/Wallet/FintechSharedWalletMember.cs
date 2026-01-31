using Entity.Entities.PermissionManagement;
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
    [Table("Fintech_SharedWalletMember")]
    public class FintechSharedWalletMember: BaseEntity
    {
        public Guid Id { get; set; }

        public Guid SharedWalletId { get; set; }

        public Guid UserId { get; set; }

    }

    public class FintechSharedWalletMemberConfiguration : IEntityTypeConfiguration<FintechSharedWalletMember>
    {
        public void Configure(EntityTypeBuilder<FintechSharedWalletMember> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).HasColumnType("uniqueidentifier");
            builder.Property(a => a.SharedWalletId).HasColumnType("uniqueidentifier").IsRequired();
            builder.Property(a => a.UserId).HasColumnType("uniqueidentifier").IsRequired();
            builder.HasIndex(x => x.CreatedDate);
            builder.Property(x => x.CreatedDate).HasColumnType("datetime").IsRequired();
            builder.Property(x => x.UpdatedDate).HasColumnType("datetime");
            builder.Property(x => x.CreatedName).HasColumnType("nvarchar(50)");
            builder.Property(x => x.Updater).HasColumnType("nvarchar(50)");
        }
    }
}
