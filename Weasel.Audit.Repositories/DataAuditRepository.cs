using Microsoft.EntityFrameworkCore;
using Weasel.Audit.Interfaces;
using Weasel.Audit.Models;
using Weasel.Audit.Services;
using Weasel.Tools.Extensions.EFCore;

namespace Weasel.Audit.Repositories;

public interface IDataAuditRepository : IStandartRepository<IAuditAction>
{
    IAuditPropertyManager PropertyManager { get; }
    Task<IAuditAction?> FindAsync(int id);
    Task<List<IAuditAction>> GetListAsync(IEnumerable<Enum> types, int entity);
    Task<List<AuditPropertyDisplayModel>> GetItemDataAsync<T>(int id);
    Task<List<AuditPropertyDisplayModel>> GetItemDataAsync(Type actionType, int id);
}

public sealed class DataAuditRepository : StandartRepository<IAuditAction>, IDataAuditRepository
{
    public IAuditPropertyManager PropertyManager { get; private set; }
    public DataAuditRepository(DbContext context, IAuditPropertyManager propertyManager) : base(context)
    {
        PropertyManager = propertyManager;
    }
    public async Task<IAuditAction?> FindAsync(int id)
        => await Context.Set<IAuditAction>().FindAsync(id);

    public async Task<List<IAuditAction>> GetListAsync(IEnumerable<Enum> types, int entity)
    {
        return await Where(x => types.Contains(x.Type) && x.EntityId == entity)
                    .OrderBy(x => x.DateTime)
                    .ToListAsync();
    }

    public async Task<List<AuditPropertyDisplayModel>> GetItemDataAsync<T>(int id)
        => await GetItemDataAsync(typeof(T), id);
    public async Task<List<AuditPropertyDisplayModel>> GetItemDataAsync(Type actionType, int id)
    {
        var item = await Context.IncludeAll(actionType).FirstOrDefaultAsync(x => x.Id == id);
        if (item == null)
        {
            return new List<AuditPropertyDisplayModel>();
        }
        return PropertyManager.GetEntityDisplayData(actionType, item);
    }
    public async Task<ActionIndexModel?> GetIndexAsync(int id)
    {
        IAuditAction? dataAction = await FindAsync(id);
        if (dataAction == null || (dataAction.OldDataId == null && dataAction.NewDataId == null))
        {
            return null;
        }
        Type actionType = dataAction.ActionType;
        var list = new List<AuditPropertyDisplayModel[]>();
        var waitList = new List<Task<ActionIndexModel[]>>();
        if (dataAction.OldDataId != null)
        {
            var oldItemTask = GetItemDataAsync(actionType, dataAction.OldDataId.Value);
            waitList.Add(oldItemTask);
        }
        if (dataAction.NewDataId != null)
        {
            var newItemTask = GetItemDataAsync(actionType, dataAction.NewDataId.Value);
            waitList.Add(newItemTask);
        }
        await Task.WhenAll(waitList);
        foreach (var task in waitList)
        {
            list.Add(task.Result);
        }
        if (list.Count == 2)
        {
            int range = list.Min(x => x.Length);
            for (int i = 0; i < range; i++)
            {
                list[1][i].Changed = !list[0][i].Equals(list[1][i]);
            }
        }
        return new ActionIndexModel()
        {
            Action = dataAction,
            Items = list,
        };
    }
}
