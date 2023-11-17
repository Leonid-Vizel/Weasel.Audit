using ClosedXML.Excel;
using Weasel.Export.Common.Interfaces.Headers;
using Weasel.Export.Common.Models;

namespace Weasel.Export.Common.Exporters;

public abstract class CustomSingularExporter<TModel, TRow>
    : SingularExporter<TModel>,
    ICustomHeaderExporter<TModel>
{
    private static readonly string _workSheetName = "Журнал";
    private static readonly string _tableName = "Журнал";
    public abstract string[] GetHeader(TModel data);
    public abstract StandartRow ToRow(TModel model, TRow data, ref int counter);
    public abstract IReadOnlyCollection<TRow> Transform(TModel model);
    public override byte[] Export(TModel model, bool adjust = true, bool center = true, bool wrap = true)
    {
        var data = Transform(model);
        using (XLWorkbook workbook = new XLWorkbook())
        {
            IXLWorksheet worksheet = workbook.Worksheets.Add(_workSheetName);
            string[] header = GetHeader(model);
            IXLTable table = worksheet.Range(1, 1, data.Count + 1, header.Length).CreateTable(_tableName);
            table.Cell(1, 1).InsertData(header, true);
            int counter = 1;
            foreach (var rowData in data)
            {
                var standart = ToRow(model, rowData, ref counter);
                var range = table.Cell(++counter, 1).InsertData(standart.Cells, true);
                if (standart.Color != null)
                {
                    range.Style.Fill.BackgroundColor = standart.Color;
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
