using System.ComponentModel.DataAnnotations;
using Weasel.Audit.Example.Models.Data.Audits;
using Weasel.Audit.Example.Models.Enums.Audit;
using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Example.Models.Data.ImportantDatas;

public sealed class ImportantDataAction : ImportantDataBase, IAuditResult<AuditAction, AuditRow, AuditType>
{
    [Key]
    public int Id { get; set; }
    public int ActionId { get; set; }
    public AuditAction Action { get; set; } = null!;
    public ImportantDataAction() : base() { }
    public ImportantDataAction(ImportantData model) : base(model) { }
}
