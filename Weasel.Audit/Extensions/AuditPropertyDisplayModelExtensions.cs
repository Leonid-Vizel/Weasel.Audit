using Weasel.Audit.Interfaces;
using Weasel.Audit.Models;

namespace Weasel.Audit.Extensions;

public static class AuditPropertyDisplayModelExtensions
{
    public static void Check<TAction, TRow, TEnum>(this AuditHistoryModel<TAction, TRow, TEnum> model)
        where TAction : class, IAuditAction<TRow, TEnum>
        where TRow : IAuditRow<TEnum>
        where TEnum : struct, Enum
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
