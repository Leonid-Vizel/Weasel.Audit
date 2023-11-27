using Microsoft.EntityFrameworkCore;
using System.Drawing;
using Weasel.Audit.Enums;
using Weasel.Audit.Extensions;
using Weasel.Audit.Interfaces;
using Weasel.Audit.Models;
using Weasel.Audit.Services;

namespace Weasel.Audit.Repositories;

public interface IAuditActionRepository<TAuditAction, TEnum, TColor> : IStandartRepository<TAuditAction>
    where TAuditAction : class, IAuditAction<TEnum>
    where TEnum : struct, Enum
    where TColor : struct, Enum
{
    IAuditSchemeManager<TEnum, TColor> SchemeManager { get; }
    IAuditPropertyManager PropertyManager { get; }
    Task<IAuditAction<TEnum>?> FindAsync(int id);
    Task<AuditIndexModel<TAuditAction, TEnum>?> GetIndexAsync(int id);
    Task<AuditHistoryModel<TAuditAction, TEnum>?> GetHistoryAsync(string name, string entityId);
}

public sealed class AuditActionRepository<TAuditAction, TEnum, TColor> : StandartRepository<TAuditAction>, IAuditActionRepository<TAuditAction, TEnum, TColor>
    where TAuditAction : class, IAuditAction<TEnum>
    where TEnum : struct, Enum
    where TColor : struct, Enum
{
    public IAuditSchemeManager<TEnum, TColor> SchemeManager { get; private set; }
    public IAuditPropertyManager PropertyManager { get; private set; }
    public AuditActionRepository(DbContext context, IAuditSchemeManager<TEnum, TColor> schemeManager, IAuditPropertyManager propertyManager) : base(context)
    {
        SchemeManager = schemeManager;
        PropertyManager = propertyManager;
    }
    public async Task<IAuditAction<TEnum>?> FindAsync(int id)
        => await Set.FindAsync(id);
    public async Task<AuditIndexModel<TAuditAction, TEnum>?> GetIndexAsync(int id)
    {
        var found = await Set.FindAsync(id);
        if (found == null)
        {
            return null;
        }
        var description = SchemeManager.GetAuditEnumDescription(found.Type);
        if (description == null)
        {
            return null;
        }
        var rowsQuery = Context.IncludeAuditResult<TAuditAction, TEnum>(description.Type);
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
                return new AuditInfoModel<TAuditAction, TEnum>()
                {
                    Items = data,
                    Action = row.Action,
                };
        }
        var olderRow = await rowsQuery.OrderByDescending(x => x.Id).FirstOrDefaultAsync(x => x.Action.EntityId == row.Action.EntityId && x.Id < row.Id);
        if (olderRow == null)
        {
            return new AuditInfoModel<TAuditAction, TEnum>()
            {
                Items = data,
                Action = row.Action,
            };
        }
        var olderRowData = PropertyManager.GetEntityDisplayData(description.Type, olderRow);
        return new AuditUpdateModel<TAuditAction, TEnum>()
        {
            Old = olderRowData,
            Update = data,
            Action = row.Action,
        };
    }
    public async Task<AuditHistoryModel<TAuditAction, TEnum>?> GetHistoryAsync(string name, string entityId)
    {
        var type = SchemeManager.GetTypeBySearchName(name);
        if (type == null)
        {
            return null;
        }
        var model = new AuditHistoryModel<TAuditAction, TEnum>()
        {
            Type = type,
            EntityId = entityId,
            TypeName = name,
        };
        var rowsQuery = Context.IncludeAuditResult<TAuditAction, TEnum>(type);
        if (rowsQuery == null)
        {
            return null;
        }
        var rows = await rowsQuery.Where(x=>x.Action.EntityId == entityId).OrderBy(x => x.Action.DateTime).ThenBy(x => x.Id).ToListAsync();
        foreach (var row in rows)
        {
            model.Actions.Add(new AuditHistoryStateModel<TAuditAction, TEnum>()
            {
                Action = row.Action,
                Items = PropertyManager.GetEntityDisplayData(type, row)
            });
        }
        model.Check();
        return model;
    }
}
