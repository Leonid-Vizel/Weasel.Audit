using Weasel.Audit.Interfaces;

namespace Weasel.Models.Audit;

public sealed class ActionHistoryPageModel
{
    public IAuditAction Action { get; set; } = null!;
    public ActionIndexItem[] Items { get; set; } = null!;
}
