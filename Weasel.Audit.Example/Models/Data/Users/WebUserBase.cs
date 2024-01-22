using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Weasel.Audit.Attributes.AuditUpdate;
using Weasel.Audit.Attributes.Display;
using Weasel.Audit.Example.Models.Enums.Users;
using Weasel.Tools.Extensions.Common;

namespace Weasel.Audit.Example.Models.Data.Users;

public abstract class WebUserBase
{
    [DisplayName("Имя")]
    [Required(ErrorMessage = "Укажите имя!")]
    [MaxLength(500, ErrorMessage = "Максимальная длина - 500 символов!")]
    public string FirstName { get; set; } = null!;
    [DisplayName("Фамилия")]
    [Required(ErrorMessage = "Укажите фамилию!")]
    [MaxLength(500, ErrorMessage = "Максимальная длина - 500 символов!")]
    public string SecondName { get; set; } = null!;
    [DisplayName("Отчество")]
    [Required(ErrorMessage = "Укажите отчество!")]
    [MaxLength(500, ErrorMessage = "Максимальная длина - 500 символов!")]
    public string ThirdName { get; set; } = null!;
    [DisplayName("Должность")]
    [Required(ErrorMessage = "Укажите должность!")]
    [MaxLength(1000, ErrorMessage = "Максимальная длина - 1000 символов!")]
    public string Position { get; set; } = null!;
    [DisplayName("Системная должность")]
    [Required(ErrorMessage = "Укажите системную должность!")]
    [Range(0, 2, ErrorMessage = "Укажите системную должность!")]
    public UserRole Role { get; set; }
    [DisplayName("Пользователь уволен?")]
    public bool Fired { get; set; }

    [NotMapped]
    [IgnoreAuditDisplay]
    [IgnoreAuditUpdate]
    public string FullName => $"{SecondName} {FirstName} {ThirdName}";

    public WebUserBase() : base() { }
    public WebUserBase(WebUserBase model) : this()
    {
        FirstName = model.FirstName;
        SecondName = model.SecondName;
        ThirdName = model.ThirdName;
        Position = model.Position;
        Role = model.Role;
        Fired = model.Fired;
    }

    public static readonly SelectListItem[] RolesSelectItems = Enum.GetValues<UserRole>().Cast<UserRole>().Select(x => new SelectListItem()
    {
        Value = ((int)x).ToString(),
        Text = x.GetDisplayNameNonNull(),
    }).ToArray();
}