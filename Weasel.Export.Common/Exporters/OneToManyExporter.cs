using ClosedXML.Excel;
using Weasel.Export.Common.Interfaces;
using Weasel.Export.Common.Interfaces.Headers;
using Weasel.Export.Common.Interfaces.Rows;
using Weasel.Export.Common.Models;

namespace Weasel.Export.Common.Exporters;

public abstract class OneToManyExporter<T> : IStandartHeaderExporter, IOneToManyRowExporter<T>, IPluralExporter<T>
{
    private static readonly string _workSheetName = "Журнал";
    private static readonly string _tableName = "Журнал";
    public abstract string[] GetHeader();
    public abstract IReadOnlyCollection<StandartRow> ToRows(T data, ref int counter);
    public byte[] Export(IReadOnlyCollection<T> data, bool adjust = true, bool center = true, bool wrap = true)
    {
        using (XLWorkbook workbook = new XLWorkbook())
        {
            IXLWorksheet worksheet = workbook.Worksheets.Add(_workSheetName);
            string[] header = GetHeader();
            IXLTable table = worksheet.Range(1, 1, data.Count + 1, header.Length).CreateTable(_tableName);
            table.Cell(1, 1).InsertData(header, true);
            int counter = 1;
            int rowCount = 2;
            foreach (var rowData in data)
            {
                var rows = ToRows(rowData, ref counter);
                foreach (var row in rows)
                {
                    var range = table.Cell(rowCount++, 1).InsertData(row.Cells, true);
                    if (row.Color != null)
                    {
                        range.Style.Fill.BackgroundColor = row.Color;
                    }
                }
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
