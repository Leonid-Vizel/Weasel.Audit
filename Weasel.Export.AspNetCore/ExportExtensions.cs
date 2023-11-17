using Microsoft.AspNetCore.Mvc;

namespace Weasel.Export.AspNetCore;

public static class ExportExtensions
{
    private static readonly string _xlMimeFormat = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public static FileContentResult XlFile(this Controller controller, byte[] result, string name)
        => controller.File(result, _xlMimeFormat, $"{name}.xlsx");
}
