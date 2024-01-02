using Microsoft.EntityFrameworkCore;

namespace Weasel.Audit.Example;

public sealed class MainDbContext : DbContext
{
    public MainDbContext(DbContextOptions options) : base(options) { }

    public DbSet<AuditAction> AuditActions { get; set; }
    public DbSet<WebUser> Users { get; set; }
    public DbSet<WebUserAction> UserActions { get; set; }
}
