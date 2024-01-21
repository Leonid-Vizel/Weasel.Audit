using System.ComponentModel.DataAnnotations;

namespace Weasel.Audit.Example.Models.Enums.Users;

public enum UserRole
{
    [Display(Name = "Пользователь")]
    Standart,
    [Display(Name = "Администратор")]
    Admin,
    [Display(Name = "Система")]
    System,
}