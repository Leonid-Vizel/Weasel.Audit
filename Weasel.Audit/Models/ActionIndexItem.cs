namespace Weasel.Audit.Models;

public sealed class ActionIndexItem
{
    public ActionIndexItem() : base() { }
    public ActionIndexItem(string name) : this()
    {
        Name = name;
    }

    public string Name { get; set; } = null!;
    public object? Value { get; set; }
    public bool Changed { get; set; }

    public bool Equals(ActionIndexItem obj)
    {
        ActionIndexItem[]? oldArray = Value as ActionIndexItem[];
        ActionIndexItem[]? newArray = obj.Value as ActionIndexItem[];
        if (oldArray != null && newArray != null)
        {
            int range = Math.Min(oldArray.Length, newArray.Length);
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