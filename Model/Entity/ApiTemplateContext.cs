using Entity.Entities.Account;
using Entity.Entities.PermissionManagement;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Entity.Entities;

namespace Entity;

public partial class ApiTemplateContext : DbContext
{
    public const string USP_GetPermission = "USP_GetPermission";
    public ApiTemplateContext()
    {
    }

    public ApiTemplateContext(DbContextOptions<ApiTemplateContext> options)
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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
       => base.OnConfiguring(optionsBuilder);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<BaseEntity>();
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
