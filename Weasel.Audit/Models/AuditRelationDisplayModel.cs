namespace Weasel.Audit.Models;

public sealed class AuditRelationDisplayModel : AuditPropertyDisplayModel
{
    public Type Type { get; set; }
    public AuditRelationDisplayModel(string name, Type type) : base(name)
    {
        Type = type;
    }
}
