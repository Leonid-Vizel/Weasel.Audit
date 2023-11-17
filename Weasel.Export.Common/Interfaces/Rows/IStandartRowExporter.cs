using Weasel.Export.Common.Models;

namespace Weasel.Export.Common.Interfaces.Rows;

public interface IStandartRowExporter<T>
{
    StandartRow ToRow(T data, ref int counter);
}
