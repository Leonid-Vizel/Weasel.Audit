namespace Weasel.Audit.Interfaces;

public interface IAuditAction<TRow, TEnum> : IIntKeyedEntity
    where TRow : IAuditRow<TEnum>
    where TEnum : struct, Enum
{
    int RowId { get; set; }
    TRow Row { get; }
    string EntityId { get; set; }
}
