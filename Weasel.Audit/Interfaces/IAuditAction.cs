namespace Weasel.Audit.Interfaces;

public interface IAuditAction<TEnum> : IIntKeyedEntity
    where TEnum : struct, Enum
{
    string EntityId { get; set; }
	TEnum Type { get; set; }
    DateTime DateTime { get; set; }
}
