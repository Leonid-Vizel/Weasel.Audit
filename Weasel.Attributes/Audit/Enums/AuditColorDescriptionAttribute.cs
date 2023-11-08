namespace Weasel.Attributes.Audit.Enums;

public sealed class AuditColorDescriptionAttribute : Attribute
{
    public string? TableTrClass { get; private set; }
    public string? TableTrInlineStyles { get; private set; }
    public string? HistoryBlockClass { get; private set; }
    public string? HistoryInlineStyles { get; private set; }
    public AuditColorDescriptionAttribute(string? tableTrClass = null, string? tableTrInlineStyles = null, string? historyBlockClass = null, string? historyInlineStyles = null)
    {
        TableTrClass = tableTrClass;
        TableTrInlineStyles = tableTrInlineStyles;
        HistoryBlockClass = historyBlockClass;
        HistoryInlineStyles = historyInlineStyles;
    }
}
