using Weasel.Tools.Extensions.Common;

namespace Weasel.Audit.Models;

public sealed class ActionHistoryModel
{
    public Type Type { get; set; } = null!;
    public string TypeName { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public List<ActionIndexModel> Actions { get; set; } = null!;

    public ActionHistoryModel() : base()
    {
        Actions = new List<ActionIndexModel>();
    }
    public ActionHistoryModel(Type type, string entityId) : this()
    {
        EntityId = entityId;
        Type = type;
        TypeName = Type.GetDisplayName() ?? type.Name;
    }
}
