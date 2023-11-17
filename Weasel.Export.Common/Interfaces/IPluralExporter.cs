namespace Weasel.Export.Common.Interfaces;

public interface IPluralExporter<T>
{
    byte[] Export(IReadOnlyCollection<T> data, bool adjust = true, bool center = true, bool wrap = true);
}
