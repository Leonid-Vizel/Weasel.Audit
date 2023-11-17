using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Models;

public sealed class ActionIndexModel
{
    public IAuditAction Action { get; set; } = null!;
    public List<AuditPropertyDisplayModel>[] Items { get; set; } = null!;
}
