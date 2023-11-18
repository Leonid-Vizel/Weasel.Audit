using Weasel.Audit.Interfaces;
using Weasel.Tools.Extensions.Common;

namespace Weasel.Audit.Models;

public sealed class AuditHistoryModel<TAuditAction>
    where TAuditAction : class, IAuditAction
{
    public Type Type { get; set; } = null!;
    public string TypeName { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public List<AuditHistoryStateModel<TAuditAction>> Actions { get; set; } = null!;

    public AuditHistoryModel() : base()
    {
        Actions = new List<AuditHistoryStateModel<TAuditAction>>();
    }
    public AuditHistoryModel(Type type, string entityId) : this()
    {
        EntityId = entityId;
        Type = type;
        TypeName = Type.GetDisplayName() ?? type.Name;
    }
}
