using ClosedXML.Excel;

namespace Weasel.Export.Common;

public static class ExportUtilities
{
    public static void ApplyRules(this IXLWorksheet worksheet, bool adjust, bool center, bool wrap)
    {
        if (adjust)
        {
            worksheet.Columns().AdjustToContents(1, 1);
        }
        worksheet.Style.Alignment.WrapText = wrap;
        if (center)
        {
            worksheet.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            worksheet.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        }
    }
}
