using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using Weasel.Audit.Example.Models.Enums.Audit;
using Weasel.Audit.Interfaces;
using Weasel.Audit.Services;

namespace Weasel.Audit.Example.Models.Data.Audits;

public class AuditAdditionalData
{
    public string? OverrideLogin { get; set; }
    public AuditColor? OverrideColor { get; set; }
    public int? UserId { get; set; }
    public AuditAdditionalData() : base() { }
    public AuditAdditionalData(int? userId = null, AuditColor? color = null, string? login = null) : this()
    {
        OverrideLogin = login;
        OverrideColor = color;
        UserId = userId;
    }
    public AuditAdditionalData(AuditAdditionalData? data = null) :
        this(data?.UserId, data?.OverrideColor, data?.OverrideLogin)
    { }
}

[ValidateNever]
public sealed class AuditActionFactory : IAuditActionFactory<AuditAction, AuditRow, AuditType>
{
    public AuditAction CreateAuditAction(AuditRow row, string entityId, object? additional = null)
        => new AuditAction(row, entityId);
}

[ValidateNever]
public sealed class AuditAction : IAuditAction<AuditRow, AuditType>
{
    [Key]
    public int Id { get; set; }
    public string EntityId { get; set; } = null!;
    public int RowId { get; set; }
    public AuditRow Row { get; set; } = null!;

    public AuditAction() : base() { }
    public AuditAction(AuditRow row, string entityId)
    {
        Row = row;
        EntityId = entityId;
    }
}
