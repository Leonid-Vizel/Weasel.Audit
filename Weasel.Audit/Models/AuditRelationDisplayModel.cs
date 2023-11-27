namespace Weasel.Audit.Models;

public sealed class AuditRelationDisplayModel : AuditPropertyDisplayModel
{
    public Type Type { get; set; }
    public AuditRelationDisplayModel(string name, Type type, object? value = null) : base(name)
    {
        Type = type;
        Value = value?.ToString();
    }
}
