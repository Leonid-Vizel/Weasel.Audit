using Weasel.Audit.Interfaces;
using Weasel.Tools.Extensions.Common;

namespace Weasel.Audit.Models;

public sealed class AuditHistoryModel<TAction, TRow, TEnum>
    where TAction : class, IAuditAction<TRow, TEnum>
    where TRow : IAuditRow<TEnum>
	where TEnum : struct, Enum
{
    public Type Type { get; set; } = null!;
    public string TypeName { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public List<AuditHistoryStateModel<TAction, TRow, TEnum>> Actions { get; set; } = null!;

    public AuditHistoryModel() : base()
    {
        Actions = [];
    }
    public AuditHistoryModel(Type type, string entityId) : this()
    {
        EntityId = entityId;
        Type = type;
        TypeName = Type.GetDisplayName() ?? type.Name;
    }
}
