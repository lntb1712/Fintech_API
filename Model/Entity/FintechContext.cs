using Entity.Entities.Account;
using Entity.Entities.PermissionManagement;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Entity.Entities;
using Entity.Entities.Wallet;

namespace Entity;

public partial class FintechContext : DbContext
{
    public const string USP_GetPermission = "USP_GetPermission";

    public FintechContext()
    {
    }

    public FintechContext(DbContextOptions<FintechContext> options)
        : base(options)
    {
    }

    public virtual DbSet<SysAccount> SysAccounts { get; set; }
    public virtual DbSet<SysDevice> SysDevices { get; set; }
    public virtual DbSet<SysRole> SysRoles { get; set; }
    public virtual DbSet<SysUserRole> SysUserRoles { get; set; }
    public virtual DbSet<SysActivity> SysUsers { get; set; }
    public virtual DbSet<SysRoleActivity> SysRoleActivities { get; set; }
    public virtual DbSet<SysUserActivity> SysUserActivities { get; set; }

    // mail template
    public virtual DbSet<SysMailTemplate> SysMailTemplates { get; set; }

    // qltb
    public virtual DbSet<SysAppVersion> SysAppVersions { get; set; }

    public virtual DbSet<SysUserDevice> SysUserDevices { get; set; }
    public virtual DbSet<SysNotification> SysNotifications { get; set; }

    //wallet
    public virtual DbSet<FintechWallet> FintechWallets { get; set; }

    public virtual DbSet<FintechTransaction> FintechTransactions { get; set; }

    public virtual DbSet<FintechTransactionTag> FintechTransactionTags { get; set; }

    public virtual DbSet<FintechTransfer> FintechTransfers { get; set; }

    public virtual DbSet<FintechGoal> FintechGoals { get; set; }

    public virtual DbSet<FintechBudget> FintechBudgets { get; set; }

    public virtual DbSet<FintechTag> FintechTags { get; set; }

    public virtual DbSet<FintechSharedWallet> FintechSharedWallets { get; set; }

    public virtual DbSet<FintechSharedWalletMember> FintechSharedWalletMembers { get; set; }

    public virtual DbSet<FintechRecurringTransaction> FintechRecurringTransactions { get; set; }

    public virtual DbSet<FintechCategory> FintechCategories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
       => base.OnConfiguring(optionsBuilder);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<BaseEntity>();
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}