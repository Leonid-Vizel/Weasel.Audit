using Weasel.Audit.Interfaces;
using Weasel.Tools.Extensions.Common;

namespace Weasel.Audit.Models;

public sealed class AuditHistoryModel<TAuditAction, TEnum>
    where TAuditAction : class, IAuditAction<TEnum>
	where TEnum : struct, Enum
{
    public Type Type { get; set; } = null!;
    public string TypeName { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public List<AuditHistoryStateModel<TAuditAction, TEnum>> Actions { get; set; } = null!;

    public AuditHistoryModel() : base()
    {
        Actions = new List<AuditHistoryStateModel<TAuditAction, TEnum>>();
    }
    public AuditHistoryModel(Type type, string entityId) : this()
    {
        EntityId = entityId;
        Type = type;
        TypeName = Type.GetDisplayName() ?? type.Name;
    }
}
