using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Weasel.Audit.Example.Models.Data.Users;
using Weasel.Audit.Example.Models.Enums.Audit;
using Weasel.Audit.Interfaces;
using Weasel.Audit.Services;

namespace Weasel.Audit.Example.Models.Data.Audits;

public sealed class AuditRow : AuditAdditionalData, IAuditRow<AuditType>
{
    public int Id { get; set; }
    public AuditType Type { get; set; }
    public DateTime DateTime { get; set; }
    public bool Grouped { get; set; }
    public WebUser? User { get; set; }

    public AuditRow() : base() { }
    public AuditRow(AuditType type, bool grouped, object? additional = null)
        : base(additional as AuditAdditionalData)
    {
        Type = type;
        Grouped = grouped;
        DateTime = DateTime.Now;
    }
}

[ValidateNever]
public sealed class AuditRowFactory : IAuditRowFactory<AuditRow, AuditType>
{
    public AuditRow CreateAuditRow(AuditType type, bool grouped = false, object? additional = null)
        => new AuditRow(type, grouped, additional);
}
