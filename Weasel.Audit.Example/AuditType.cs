using Weasel.Audit.Attributes.Enums;
using Weasel.Audit.Enums;

namespace Weasel.Audit.Example;

public enum AuditType
{
    [AuditDesc<Journal, AuditColor>(Journal.WebUsers, "Добавлен пользователь", AuditColor.Green, AuditScheme.Create, typeof(WebUserAction))]
    WebUserCreated,
    [AuditDesc<Journal, AuditColor>(Journal.WebUsers, "Изменён пользователь", AuditColor.Yellow, AuditScheme.Update, typeof(WebUserAction))]
    WebUserEdited,
    [AuditDesc<Journal, AuditColor>(Journal.WebUsers, "Пользователь уволен (отключён)", AuditColor.Red, AuditScheme.CustomUpdate, typeof(WebUserAction))]
    WebUserFired,
}
