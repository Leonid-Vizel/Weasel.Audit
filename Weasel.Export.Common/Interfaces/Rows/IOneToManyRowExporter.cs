using Weasel.Export.Common.Models;

namespace Weasel.Export.Common.Interfaces.Rows;

public interface IOneToManyRowExporter<T>
{
    IReadOnlyCollection<StandartRow> ToRows(T data, ref int counter);
}
