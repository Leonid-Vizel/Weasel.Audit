namespace Weasel.Tools.Extensions.EFCore;

public struct IncludeAllCacheKey
{
    public Type Type { get; private set; }
    public int Depth { get; private set; }
    public IncludeAllCacheKey(Type type, int depth)
    {
        Type = type;
        Depth = depth;
    }
}
