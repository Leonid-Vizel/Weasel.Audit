using Microsoft.EntityFrameworkCore;
using Weasel.Audit.Extensions;
using Weasel.Audit.Interfaces;
using Weasel.Audit.Models;
using Weasel.Audit.Services;

namespace Weasel.Audit.Repositories;

public interface IDataAuditRepository : IStandartRepository<IAuditAction>
{
    IAuditPropertyManager PropertyManager { get; }
    Task<IAuditAction?> FindAsync(int id);
    Task<List<IAuditAction>> GetListAsync(IEnumerable<Enum> types, string entity);
    Task<List<AuditPropertyDisplayModel>> GetItemDataAsync<T>(int id);
    Task<List<AuditPropertyDisplayModel>> GetItemDataAsync(Type actionType, int id);
    Task<ActionIndexModel?> GetIndexAsync(int id);
}

public sealed class DataAuditRepository : StandartRepository<IAuditAction>, IDataAuditRepository
{
    public IAuditSchemeManager SchemeManager { get; private set; }
    public IAuditPropertyManager PropertyManager { get; private set; }
    public IServiceProvider ServiceProvider { get; private set; }
    public DataAuditRepository(DbContext context, IAuditSchemeManager schemeManager, IAuditPropertyManager propertyManager, IServiceProvider serviceProvider) : base(context)
    {
        SchemeManager = schemeManager;
        PropertyManager = propertyManager;
        ServiceProvider = serviceProvider;
    }
    public async Task<IAuditAction?> FindAsync(int id)
        => await Context.Set<IAuditAction>().FindAsync(id);
    public async Task<List<IAuditAction>> GetListAsync(IEnumerable<Enum> types, string entity)
    {
        return await Where(x => types.Contains(x.Type) && x.EntityId == entity)
                    .OrderBy(x => x.DateTime)
                    .ToListAsync();
    }
    public async Task<List<AuditPropertyDisplayModel>> GetItemDataAsync<T>(int id)
        => await GetItemDataAsync(typeof(T), id);
    public async Task<List<AuditPropertyDisplayModel>> GetItemDataAsync(Type actionType, int id)
    {
        var item = await Context.IncludeAllIntKeyed(actionType).FirstOrDefaultAsync(x => x.Id == id);
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
        Type? actionType = SchemeManager.GetAuditEnumActionType(dataAction.Type);
        if (actionType == null)
        {
            return null;
        }
        var list = new List<List<AuditPropertyDisplayModel>>();
        if (dataAction.OldDataId != null)
        {
            var oldItem = await GetItemDataAsync(actionType, dataAction.OldDataId.Value);
            if (oldItem != null)
            {
                list.Add(oldItem);
            }
        }
        if (dataAction.NewDataId != null)
        {
            var newItem = await GetItemDataAsync(actionType, dataAction.NewDataId.Value);
            if (newItem != null)
            {
                list.Add(newItem);
            }
        }
        if (list.Count == 2)
        {
            int range = list.Min(x => x.Count);
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
