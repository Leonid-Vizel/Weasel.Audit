using Microsoft.EntityFrameworkCore;
using Weasel.Audit.Example.Models.Data.Audits;
using Weasel.Audit.Example.Models.Data.ImportantDatas;
using Weasel.Audit.Example.Models.Data.Users;

namespace Weasel.Audit.Example.Data;

public sealed class MainDbContext : DbContext
{
    public MainDbContext(DbContextOptions options) : base(options) { }

    public DbSet<AuditRow> AuditRows { get; set; }
    public DbSet<AuditAction> AuditActions { get; set; }
    public DbSet<WebUser> Users { get; set; }
    public DbSet<WebUserAction> UserActions { get; set; }
    public DbSet<ImportantData> ImportantData { get; set; }
    public DbSet<ImportantDataAction> ImportantDataActions { get; set; }
}
