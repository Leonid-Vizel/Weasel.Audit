namespace Weasel.Export.Common.Exporters;

public abstract class SingularExporter<T>
{
    public abstract byte[] Export(T data, bool adjust = true, bool center = true, bool wrap = true);
}
