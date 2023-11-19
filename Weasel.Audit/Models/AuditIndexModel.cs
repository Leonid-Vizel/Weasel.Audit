using System;
using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Models;

public abstract class AuditIndexModel<TAuditAction, TEnum>
    where TAuditAction : class, IAuditAction<TEnum>
	where TEnum : struct, Enum
{
    public TAuditAction Action { get; set; } = null!;
}
