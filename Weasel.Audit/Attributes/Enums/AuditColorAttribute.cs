namespace Weasel.Audit.Attributes.Enums;

[AttributeUsage(AttributeTargets.Field)]
public sealed class AuditColorAttribute : Attribute
{
    public string? TableTrClass { get; private set; }
    public string? TableTrInlineStyles { get; private set; }
    public string? HistoryBlockClass { get; private set; }
    public string? HistoryInlineStyles { get; private set; }
    public AuditColorAttribute(string? tableTrClass = null, string? tableTrInlineStyles = null, string? historyBlockClass = null, string? historyInlineStyles = null)
    {
        TableTrClass = tableTrClass;
        TableTrInlineStyles = tableTrInlineStyles;
        HistoryBlockClass = historyBlockClass;
        HistoryInlineStyles = historyInlineStyles;
    }
}
