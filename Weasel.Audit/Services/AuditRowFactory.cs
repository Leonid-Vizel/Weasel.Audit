using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Services;

public interface IAuditRowFactory<TRow, TEnum>
    where TRow : IAuditRow<TEnum>
    where TEnum : struct, Enum
{
    public TRow CreateAuditRow(TEnum type, bool grouped = false, object? additional = null);
}