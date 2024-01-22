namespace Weasel.Audit.Interfaces;

public interface IAuditAction<TRow, TEnum> : IIntKeyedEntity
    where TRow : IAuditRow<TEnum>
    where TEnum : struct, Enum
{
    int RowId { get; set; }
    TRow Row { get; set; }
    string EntityId { get; set; }
}
