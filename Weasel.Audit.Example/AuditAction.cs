using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using Weasel.Audit.Interfaces;
using Weasel.Audit.Services;

namespace Weasel.Audit.Example;

public class AuditAdditionalData
{
    public string? OverrideLogin { get; set; }
    public AuditColor? OverrideColor { get; set; }
    public int? UserId { get; set; }
    public AuditAdditionalData() : base() { }
    public AuditAdditionalData(AuditAdditionalData? data = null) : this()
    {
        OverrideLogin = data?.OverrideLogin;
        OverrideColor = data?.OverrideColor;
        UserId = data?.UserId;
    }

    public AuditAdditionalData(int? user = null, AuditColor? color = null, string? login = null) : this()
    {
        OverrideLogin = login;
        OverrideColor = color;
        UserId = user;
    }
}

[ValidateNever]
public sealed class AuditActionFactory : IAuditActionFactory<AuditAction, AuditType>
{
    public AuditAction CreateAuditAction(AuditType type, string entityId, object? additional = null)
        => new AuditAction(type, entityId, additional);
}

[ValidateNever]
public sealed class AuditAction : AuditAdditionalData, IAuditAction<AuditType>
{
    [Key]
    public int Id { get; set; }
    public string EntityId { get; set; } = null!;
    public AuditType Type { get; set; }
    public DateTime DateTime { get; set; }
    public WebUser? User { get; set; }
    public AuditAction() : base() { }
    public AuditAction(AuditType type, string entityId, object? additional = null)
        : base(additional as AuditAdditionalData)
    {
        Type = type;
        EntityId = entityId;
        DateTime = DateTime.Now;
    }
}
