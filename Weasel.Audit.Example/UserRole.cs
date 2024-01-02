using System.ComponentModel.DataAnnotations;

namespace Weasel.Audit.Example;

public enum UserRole
{
    [Display(Name = "Пользователь")]
    Standart,
    [Display(Name = "Администратор")]
    Admin,
    [Display(Name = "Система")]
    System,
}