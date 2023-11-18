using Weasel.Audit.Interfaces;
using Weasel.Audit.Models;

namespace Weasel.Audit.Extensions;

public static class AuditPropertyDisplayModelExtensions
{
    public static void Check<TAuditAction>(this AuditHistoryModel<TAuditAction> model)
        where TAuditAction : class, IAuditAction
    {
        if (model.Actions.Count >= 2)
        {
            int range = model.Actions.Min(x => x.Items.Count);
            for (int j = 0; j < model.Actions.Count - 1; j++)
            {
                for (int i = 0; i < range; i++)
                {
                    var pastItem = model.Actions[j].Items[i];
                    var changeItem = model.Actions[j + 1].Items[i];
                    changeItem.Changed = !pastItem.Equals(changeItem);
                }
            }
        }
    }

}
