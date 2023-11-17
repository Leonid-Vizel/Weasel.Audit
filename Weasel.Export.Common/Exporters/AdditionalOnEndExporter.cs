using ClosedXML.Excel;
using Weasel.Export.Common.Interfaces;
using Weasel.Export.Common.Interfaces.Headers;
using Weasel.Export.Common.Interfaces.Rows;
using Weasel.Export.Common.Models;

namespace Weasel.Export.Common.Exporters;

public abstract class AdditionalOnEndExporter<T> : IStandartHeaderExporter, IStandartRowExporter<T>, IManyToOneRowExporter<T>, IPluralExporter<T>
{
    private static readonly string _workSheetName = "Журнал";
    private static readonly string _tableName = "Журнал";
    public abstract string[] GetHeader();
    public abstract StandartRow ToRow(IReadOnlyCollection<T> data, ref int counter);
    public abstract StandartRow ToRow(T data, ref int counter);
    public byte[] Export(IReadOnlyCollection<T> data, bool adjust = true, bool center = true, bool wrap = true)
    {
        using (XLWorkbook workbook = new XLWorkbook())
        {
            IXLWorksheet worksheet = workbook.Worksheets.Add(_workSheetName);
            string[] header = GetHeader();
            IXLTable table = worksheet.Range(1, 1, data.Count + 1, header.Length).CreateTable(_tableName);
            table.Cell(1, 1).InsertData(header, true);
            int counter = 1;
            foreach (var rowData in data)
            {
                var standart = ToRow(rowData, ref counter);
                var range = table.Cell(++counter, 1).InsertData(standart.Cells, true);
                if (standart.Color != null)
                {
                    range.Style.Fill.BackgroundColor = standart.Color;
                }
            }
            counter++;
            var lastRow = ToRow(data, ref counter);
            var lastRange = table.Cell(counter, 1).InsertData(lastRow.Cells, true);
            if (lastRow.Color != null)
            {
                lastRange.Style.Fill.BackgroundColor = lastRow.Color;
            }
            worksheet.ApplyRules(adjust, center, wrap);
            using (MemoryStream memStream = new MemoryStream())
            {
                workbook.SaveAs(memStream);
                return memStream.ToArray();
            }
        }
    }
}
