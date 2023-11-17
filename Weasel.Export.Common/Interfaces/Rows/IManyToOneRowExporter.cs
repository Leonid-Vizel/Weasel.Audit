using Weasel.Export.Common.Models;

namespace Weasel.Export.Common.Interfaces.Rows;

public interface IManyToOneRowExporter<T>
{
    StandartRow ToRow(IReadOnlyCollection<T> data, ref int counter);
}
