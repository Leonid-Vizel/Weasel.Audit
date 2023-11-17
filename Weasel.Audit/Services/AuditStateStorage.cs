using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Services;

public struct AuditStateCacheKey
{
    public Type Type { get; private set; }
    public string EntityId { get; private set; }
    public AuditStateCacheKey(Type type, string entityId)
    {
        Type = type;
        EntityId = entityId;
    }
}
public interface IAuditStateStorage
{
    ConcurrentDictionary<AuditStateCacheKey, IIntKeyedEntity> CachedStates { get; }
    void PushState<TAudit>(string entityId, TAudit data) where TAudit : class, IIntKeyedEntity;
    void PushState<TAudit>(AuditStateCacheKey key, TAudit data) where TAudit : class, IIntKeyedEntity;
    bool TryGetValue(Type type, string entityId, [MaybeNullWhen(false)] out IIntKeyedEntity value);
    void JoinStorage(IAuditStateStorage storage);
    bool TryGetValue<TAudit>(string entityId, [MaybeNullWhen(false)] out TAudit value)
        where TAudit : class, IIntKeyedEntity;
}
public sealed class AuditStateStorage : IAuditStateStorage
{
    public ConcurrentDictionary<AuditStateCacheKey, IIntKeyedEntity> CachedStates { get; private set; }
    public AuditStateStorage()
    {
        CachedStates = new ConcurrentDictionary<AuditStateCacheKey, IIntKeyedEntity>();
    }
    public void PushState<TAudit>(string entityId, TAudit data) where TAudit : class, IIntKeyedEntity
        => PushState(new AuditStateCacheKey(typeof(TAudit), entityId), data);
    public void PushState<TAudit>(AuditStateCacheKey key, TAudit data) where TAudit : class, IIntKeyedEntity
        => CachedStates.AddOrUpdate(key, (key) => data, (key, oldValue) => data);
    public bool TryGetValue(Type type, string entityId, [MaybeNullWhen(false)] out IIntKeyedEntity value)
        => CachedStates.TryGetValue(new AuditStateCacheKey(type, entityId), out value);
    public void JoinStorage(IAuditStateStorage storage)
    {
        foreach (var stateData in storage.CachedStates)
        {
            PushState(stateData.Key, stateData.Value);
        }
        storage.CachedStates.Clear();
    }
    public bool TryGetValue<TAudit>(string entityId, [MaybeNullWhen(false)] out TAudit value)
        where TAudit : class, IIntKeyedEntity
    {
        bool flag = CachedStates.TryGetValue(new AuditStateCacheKey(typeof(TAudit), entityId), out var outVal);
#pragma warning disable CS8600
        value = (TAudit)outVal;
#pragma warning restore CS8600
        return flag;
    }
}
