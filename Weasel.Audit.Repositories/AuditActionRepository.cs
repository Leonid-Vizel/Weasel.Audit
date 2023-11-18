using Microsoft.EntityFrameworkCore;
using Weasel.Audit.Enums;
using Weasel.Audit.Extensions;
using Weasel.Audit.Interfaces;
using Weasel.Audit.Models;
using Weasel.Audit.Services;

namespace Weasel.Audit.Repositories;

public interface IAuditActionRepository<TAuditAction> : IStandartRepository<TAuditAction>
    where TAuditAction : class, IAuditAction
{
    IAuditSchemeManager SchemeManager { get; }
    IAuditPropertyManager PropertyManager { get; }
    Task<IAuditAction?> FindAsync(int id);
    Task<AuditIndexModel<TAuditAction>?> GetIndexAsync(int id);
    Task<AuditHistoryModel<TAuditAction>?> GetHistoryAsync(string name, string entityId);
}

public sealed class AuditActionRepository<TAuditAction> : StandartRepository<TAuditAction>, IAuditActionRepository<TAuditAction>
    where TAuditAction : class, IAuditAction
{
    public IAuditSchemeManager SchemeManager { get; private set; }
    public IAuditPropertyManager PropertyManager { get; private set; }
    public AuditActionRepository(DbContext context, IAuditSchemeManager schemeManager, IAuditPropertyManager propertyManager) : base(context)
    {
        SchemeManager = schemeManager;
        PropertyManager = propertyManager;
    }
    public async Task<IAuditAction?> FindAsync(int id)
        => await Set.FindAsync(id);
    public async Task<AuditIndexModel<TAuditAction>?> GetIndexAsync(int id)
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
        var rowsQuery = Context.IncludeAuditResult<TAuditAction>(description.Type);
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
                return new AuditInfoModel<TAuditAction>()
                {
                    Items = data,
                    Action = row.Action,
                };
        }
        var olderRow = await rowsQuery.FirstOrDefaultAsync(x => x.ActionId == id);
        if (olderRow == null)
        {
            return new AuditInfoModel<TAuditAction>()
            {
                Items = data,
                Action = row.Action,
            };
        }
        var olderRowData = PropertyManager.GetEntityDisplayData(description.Type, olderRow);
        return new AuditUpdateModel<TAuditAction>()
        {
            Old = olderRowData,
            Update = data,
            Action = row.Action,
        };
    }
    public async Task<AuditHistoryModel<TAuditAction>?> GetHistoryAsync(string name, string entityId)
    {
        var type = SchemeManager.GetTypeBySearchName(name);
        if (type == null)
        {
            return null;
        }
        var model = new AuditHistoryModel<TAuditAction>()
        {
            Type = type,
            EntityId = entityId,
            TypeName = name,
        };
        var rowsQuery = Context.IncludeAuditResult<TAuditAction>(type);
        if (rowsQuery == null)
        {
            return null;
        }
        var rows = await rowsQuery.OrderBy(x => x.Action.DateTime).ThenBy(x => x.Id).ToListAsync();
        foreach (var row in rows)
        {
            model.Actions.Add(new AuditHistoryStateModel<TAuditAction>()
            {
                Action = row.Action,
                Items = PropertyManager.GetEntityDisplayData(type, row)
            });
        }
        model.Check();
        return model;
    }
}
