namespace Weasel.Audit.Interfaces;

public interface IAuditAction : IIntKeyedEntity
{
    int EntityId { get; set; }
    int? UserId { get; set; }
    Enum Type { get; set; }
    int? OldDataId { get; set; }
    int? NewDataId { get; set; }
    string? OverrideLogin { get; set; }
    Enum? OverrideColor { get; set; }
    DateTime DateTime { get; set; }
}
