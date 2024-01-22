using Microsoft.EntityFrameworkCore;
using Weasel.Audit.Enums;
using Weasel.Audit.Extensions;
using Weasel.Audit.Interfaces;
using Weasel.Audit.Models;
using Weasel.Audit.Services;

namespace Weasel.Audit.Repositories;

public interface IAuditActionRepository<TAction, TRow, TEnum, TColor> : IStandartRepository<TAction>
    where TAction : class, IAuditAction<TRow, TEnum>
    where TRow : IAuditRow<TEnum>
    where TColor : struct, Enum
    where TEnum : struct, Enum
{
    IAuditSchemeManager<TEnum, TColor> SchemeManager { get; }
    IAuditPropertyManager PropertyManager { get; }
    Task<IAuditAction<TRow, TEnum>?> FindAsync(int id);
    Task<AuditIndexModel<TAction, TRow, TEnum>?> GetIndexAsync(int id);
    Task<AuditHistoryModel<TAction, TRow, TEnum>?> GetHistoryAsync(string name, string entityId);
    Task<AuditInfoModel<TAction, TRow, TEnum>?> GetHistoryStateAsync(string name, string entityId, DateTime time);
}

public sealed class AuditActionRepository<TAction, TRow, TEnum, TColor> : StandartRepository<TAction>, IAuditActionRepository<TAction, TRow, TEnum, TColor>
    where TAction : class, IAuditAction<TRow, TEnum>
    where TRow : IAuditRow<TEnum>
    where TColor : struct, Enum
    where TEnum : struct, Enum
{
    public IAuditSchemeManager<TEnum, TColor> SchemeManager { get; private set; }
    public IAuditPropertyManager PropertyManager { get; private set; }
    public AuditActionRepository(DbContext context, IAuditSchemeManager<TEnum, TColor> schemeManager, IAuditPropertyManager propertyManager) : base(context)
    {
        SchemeManager = schemeManager;
        PropertyManager = propertyManager;
    }
    public async Task<IAuditAction<TRow, TEnum>?> FindAsync(int id)
        => await Set.FindAsync(id);
    public async Task<AuditIndexModel<TAction, TRow, TEnum>?> GetIndexAsync(int id)
    {
        var found = await Set.Include(x => x.Row).FirstOrDefaultAsync(x => x.Id == id);
        if (found == null)
        {
            return null;
        }
        var description = SchemeManager.GetAuditEnumDescription(found.Row.Type);
        if (description == null)
        {
            return null;
        }
        var rowsQuery = Context.IncludeAuditResult<TAction, TRow, TEnum>(description.Type);
        if (rowsQuery == null)
        {
            return null;
        }
        var row = await rowsQuery.FirstOrDefaultAsync(x => x.ActionId == id);
        if (row == null)
        {
            return null;
        }
        var data = PropertyManager.GetEntityDisplayData(description.Type, row);
        switch (description.Scheme)
        {
            case AuditScheme.Create:
            case AuditScheme.CustomCreate:
            case AuditScheme.Delete:
            case AuditScheme.CustomDelete:
                return new AuditInfoModel<TAction, TRow, TEnum>()
                {
                    Items = data,
                    Action = row.Action,
                };
        }
        var olderRow = await rowsQuery.OrderByDescending(x => x.Id).FirstOrDefaultAsync(x => x.Action.EntityId == row.Action.EntityId && x.Id < row.Id);
        if (olderRow == null)
        {
            return new AuditInfoModel<TAction, TRow, TEnum>()
            {
                Items = data,
                Action = row.Action,
            };
        }
        var olderRowData = PropertyManager.GetEntityDisplayData(description.Type, olderRow);
        return new AuditUpdateModel<TAction, TRow, TEnum>()
        {
            Old = olderRowData,
            Update = data,
            Action = row.Action,
        };
    }
    public async Task<AuditHistoryModel<TAction, TRow, TEnum>?> GetHistoryAsync(string name, string entityId)
    {
        var type = SchemeManager.GetTypeBySearchName(name);
        if (type == null)
        {
            return null;
        }
        var model = new AuditHistoryModel<TAction, TRow, TEnum>()
        {
            Type = type,
            EntityId = entityId,
            TypeName = name,
        };
        var rowsQuery = Context.IncludeAuditResult<TAction, TRow, TEnum>(type);
        if (rowsQuery == null)
        {
            return null;
        }
        var rows = await rowsQuery.Where(x => x.Action.EntityId == entityId).OrderBy(x => x.Action.Row.DateTime).ThenBy(x => x.Id).ToListAsync();
        foreach (var row in rows)
        {
            model.Actions.Add(new AuditHistoryStateModel<TAction, TRow, TEnum>()
            {
                Action = row.Action,
                Items = PropertyManager.GetEntityDisplayData(type, row)
            });
        }
        model.Check();
        return model;
    }

    public async Task<AuditInfoModel<TAction, TRow, TEnum>?> GetHistoryStateAsync(string name, string entityId, DateTime time)
    {
        var type = SchemeManager.GetTypeBySearchName(name);
        if (type == null)
        {
            return null;
        }
        var rowsQuery = Context.IncludeAuditResult<TAction, TRow, TEnum>(type);
        if (rowsQuery == null)
        {
            return null;
        }
        var item = await rowsQuery.FirstOrDefaultAsync(x => x.Action.EntityId == entityId && x.Action.Row.DateTime <= time);
        if (item == null)
        {
            return null;
        }
        var model = new AuditInfoModel<TAction, TRow, TEnum>()
        {
            Action = item.Action,
            Items = PropertyManager.GetEntityDisplayData(type, item)
        };
        return model;
    }
}
