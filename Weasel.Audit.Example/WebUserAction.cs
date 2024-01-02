using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Weasel.Audit.Attributes.Display;
using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Example;

[ValidateNever]
public sealed class WebUserAction : WebUserBase, IAuditResult<AuditAction, AuditType>
{
    [Key]
    [IgnoreAuditDisplay]
    public int Id { get; set; }
    [IgnoreAuditDisplay]
    public int ActionId { get; set; }
    [IgnoreAuditDisplay]
    public AuditAction Action { get; set; } = null!;
    [DisplayName("Почта")]
    [Required(ErrorMessage = "Укажите почту!")]
    [MaxLength(500, ErrorMessage = "Максимальная длина - 500 символов!")]
    [EmailAddress(ErrorMessage = "Неверный формат почты!")]
    public string Login { get; set; } = null!;

    public WebUserAction() : base() { }
    public WebUserAction(WebUser model) : base(model)
    {
        Login = model.Login;
    }
}