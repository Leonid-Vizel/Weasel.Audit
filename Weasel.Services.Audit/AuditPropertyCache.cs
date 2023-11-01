using Weasel.Attributes.Audit.Formatters;
using Weasel.Attributes.Audit.Rows;
using Weasel.Enums;

namespace Weasel.Services.Audit;

public sealed class AuditPropertyCache
{
    public Type Type { get; private set; } = null!;
    public Func<object, object> Getter { get; private set; } = null!;
    public Action<object, object> Setter { get; private set; } = null!;
    public AuditValueFormatterAttribute? ValueFormatter { get; private set; }
    public AuditRowNamingRuleAttribute? RowNaming { get; private set; }
    public AuditPropertyDisplayMode DisplayMode { get; private set; }
}
