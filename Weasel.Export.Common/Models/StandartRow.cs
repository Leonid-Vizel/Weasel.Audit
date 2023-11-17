using ClosedXML.Excel;
using System.Collections.Immutable;

namespace Weasel.Export.Common.Models;

public struct StandartRow
{
    public IReadOnlyCollection<object?> Cells { get; private set; } = null!;
    public XLColor? Color { get; private set; }
    public XLHyperlink? HyperLink { get; private set; }
    public StandartRow()
    {
        Cells = ImmutableArray<object?>.Empty;
    }
    public StandartRow(IReadOnlyCollection<object?> cells)
    {
        Cells = cells;
    }
    public StandartRow(IReadOnlyCollection<object?> cells, XLColor? color)
    {
        Cells = cells;
        Color = color;
    }
    public StandartRow(IReadOnlyCollection<object?> cells, XLColor? color, XLHyperlink? link)
    {
        Cells = cells;
        Color = color;
        HyperLink = link;
    }
}
