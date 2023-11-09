using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Models;

public sealed class ActionIndexModel
{
    public IAuditAction Action { get; set; } = null!;
    public List<ActionIndexItem[]> Items { get; set; } = null!;
}
