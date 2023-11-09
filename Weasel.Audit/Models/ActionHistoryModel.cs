using Weasel.Tools.Extensions.Common;

namespace Weasel.Audit.Models;

public sealed class ActionHistoryModel
{
    public Type Type { get; set; } = null!;
    public string TypeName { get; set; } = null!;
    public int Entity { get; set; }
    public List<ActionHistoryPageModel> Actions { get; set; } = null!;

    public ActionHistoryModel() : base()
    {
        Actions = new List<ActionHistoryPageModel>();
    }
    public ActionHistoryModel(Type type, int entity) : this()
    {
        Entity = entity;
        Type = type;
        TypeName = Type.GetDisplayName() ?? type.Name;
    }
}
