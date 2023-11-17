namespace Weasel.Export.Common.Interfaces.Headers;

public interface ICustomHeaderExporter<T>
{
    string[] GetHeader(T data);
}
