using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Weasel.Audit.Attributes.AuditUpdate;
using Weasel.Audit.Example.Models.Data.Audits;
using Weasel.Audit.Example.Models.Enums.Audit;
using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Example.Models.Data.ImportantDatas;

public sealed class ImportantData : ImportantDataBase, IAuditable<ImportantDataAction, AuditAction, AuditRow, AuditType>
{
    [Key]
    [IgnoreAuditUpdate]
    public int Id { get; set; }
    public ImportantData() : base() { }
    public ImportantData(ImportantDataBase model) : base(model) { }

    public Task<ImportantDataAction> AuditAsync(DbContext context)
        => Task.FromResult(new ImportantDataAction(this));
}
