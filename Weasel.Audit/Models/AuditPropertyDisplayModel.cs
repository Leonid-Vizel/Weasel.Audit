namespace Weasel.Audit.Models;

public class AuditPropertyDisplayModel
{
    public AuditPropertyDisplayModel() : base() { }
    public AuditPropertyDisplayModel(string name) : this()
    {
        Name = name;
    }

    public string Name { get; set; } = null!;
    public object? Value { get; set; }
    public bool Changed { get; set; }

    public bool Equals(AuditPropertyDisplayModel obj)
    {
        List<AuditPropertyDisplayModel>? oldArray = Value as List<AuditPropertyDisplayModel>;
        List<AuditPropertyDisplayModel>? newArray = obj.Value as List<AuditPropertyDisplayModel>;
        if (oldArray != null && newArray != null)
        {
            int range = Math.Min(oldArray.Count, newArray.Count);
            bool equal = true;
            for (int i = 0; i < range; i++)
            {
                var old = oldArray[i];
                var update = newArray[i];
                update.Changed = !old.Equals(update);
                equal &= !update.Changed;
            }
            return equal;
        }
        return Equals(Value, obj.Value);
    }
}