namespace Weasel.Audit.Interfaces;

public interface IAuditRow<TEnum> : IIntKeyedEntity
    where TEnum : struct, Enum
{
    TEnum Type { get; set; }
    DateTime DateTime { get; set; }
    bool Grouped { get; set; }
}
