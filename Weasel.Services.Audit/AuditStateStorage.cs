using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Weasel.Audit.Interfaces;

namespace Weasel.Services.Audit;

public struct AuditStateCacheKey
{
    public Type Type { get; private set; }
    public int EntityId { get; private set; }
    public AuditStateCacheKey(Type type, int entityId)
    {
        Type = type;
        EntityId = entityId;
    }
}
public interface IAuditStateStorage
{
    ConcurrentDictionary<AuditStateCacheKey, IIntKeyedEntity> CachedStates { get; }
    void PushState<TAction>(int entityId, TAction data) where TAction : class, IIntKeyedEntity;
    void PushState<TAction>(AuditStateCacheKey key, TAction data) where TAction : class, IIntKeyedEntity;
    bool TryGetValue(Type type, int entityId, [MaybeNullWhen(false)] out IIntKeyedEntity value);
    void JoinStorage(IAuditStateStorage storage);
    bool TryGetValue<TAction>(int entityId, [MaybeNullWhen(false)] out TAction value)
        where TAction : class, IIntKeyedEntity;
}
public sealed class AuditStateStorage : IAuditStateStorage
{
    public ConcurrentDictionary<AuditStateCacheKey, IIntKeyedEntity> CachedStates { get; private set; }
    public AuditStateStorage()
    {
        CachedStates = new ConcurrentDictionary<AuditStateCacheKey, IIntKeyedEntity>();
    }
    public void PushState<TAction>(int entityId, TAction data) where TAction : class, IIntKeyedEntity
        => PushState(new AuditStateCacheKey(typeof(TAction), entityId), data);
    public void PushState<TAction>(AuditStateCacheKey key, TAction data) where TAction : class, IIntKeyedEntity
        => CachedStates.AddOrUpdate(key, (key) => data, (key, oldValue) => data);
    public bool TryGetValue(Type type, int entityId, [MaybeNullWhen(false)] out IIntKeyedEntity value)
        => CachedStates.TryGetValue(new AuditStateCacheKey(type, entityId), out value);
    public void JoinStorage(IAuditStateStorage storage)
    {
        foreach (var stateData in storage.CachedStates)
        {
            PushState(stateData.Key, stateData.Value);
        }
        storage.CachedStates.Clear();
    }
    public bool TryGetValue<TAction>(int entityId, [MaybeNullWhen(false)] out TAction value)
        where TAction : class, IIntKeyedEntity
    {
        bool flag = CachedStates.TryGetValue(new AuditStateCacheKey(typeof(TAction), entityId), out var outVal);
        value = (TAction)outVal;
        return flag;
    }
}
