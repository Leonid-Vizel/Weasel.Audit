using System.ComponentModel.DataAnnotations;
using Weasel.Audit.Attributes.Enums;

namespace Weasel.Audit.Example;

public enum AuditColor
{
    [Display(Name = "Фиолетовый")]
    [AuditColor("table-success", null, "bg-light-info", null)]
    Purple,
    [Display(Name = "Зелёный")]
    [AuditColor("table-success", null, "bg-light-success", null)]
    Green,
    [Display(Name = "Жёлтый")]
    [AuditColor("table-warning", null, "bg-light-warning", null)]
    Yellow,
    [Display(Name = "Красный")]
    [AuditColor("table-danger", null, "bg-light-danger", null)]
    Red
}