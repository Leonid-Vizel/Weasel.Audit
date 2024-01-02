using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Weasel.Audit.Attributes.AuditUpdate;
using Weasel.Audit.Interfaces;
using Weasel.Tools.Extensions.Common;

namespace Weasel.Audit.Example;

[ValidateNever]
public sealed class WebUser : WebUserBase, IAuditable<WebUserAction, AuditAction, AuditType>
{
    [Key]
    [IgnoreAuditUpdate]
    public int Id { get; set; }
    [DisplayName("Почта")]
    [Required(ErrorMessage = "Укажите почту!")]
    [EmailAddress(ErrorMessage = "Неверный формат почты!")]
    [MaxLength(500, ErrorMessage = "Максимальная длина - {0} символов!")]
    [IgnoreAuditUpdate]
    public string Login { get; set; } = null!;
    [DisplayName("Хэш пароля")]
    [Required(ErrorMessage = "Укажите пароль!")]
    [MaxLength(1000, ErrorMessage = "Максимальная длина - {0} символов!")]
    [IgnoreAuditUpdate]
    public string PasswordHash { get; set; } = null!;
    [IgnoreAuditUpdate]
    public string? RefreshToken { get; set; }
    [IgnoreAuditUpdate]
    public DateTime? RefreshExpiryTime { get; set; }
    [NotMapped]
    [IgnoreAuditUpdate]
    public string RoleString => Role.GetDisplayNameNonNull("Неизвестная роль");

    public WebUser() : base() { }
    public WebUser(WebUser model) : base(model)
    {
        Login = model.Login;
        PasswordHash = model.PasswordHash;
    }

    public Task<WebUserAction> AuditAsync(DbContext unitOfWork)
        => Task.FromResult(new WebUserAction(this));

    public SelectListItem ToItem()
        => new SelectListItem(FullName, Id.ToString());

    [IgnoreAuditUpdate]
    public static WebUser System => new WebUser()
    {
        Login = "СИСТЕМА",
        PasswordHash = "СИСТЕМА",
        FirstName = "СИСТЕМА",
        SecondName = "СИСТЕМА",
        ThirdName = "СИСТЕМА",
        Position = "СИСТЕМА",
        Role = UserRole.System
    };
    [IgnoreAuditUpdate]
    public static WebUser DefaultAdmin => new WebUser()
    {
        Login = "admin@pvsystem24.ru",
        PasswordHash = "9AIlL#1H50oT~PG^j%CgS@k17k5T1",
        SecondName = "Админов",
        FirstName = "Админ",
        ThirdName = "Админович",
        Position = "Администратор",
        Role = UserRole.Admin
    };

    public List<Claim> GetClaims()
    {
        return new List<Claim>()
        {
            new Claim("id", Id.ToString()),
            new Claim("login", Login),
            new Claim("name", FullName),
            new Claim("firstName", FirstName),
            new Claim("secondName", SecondName),
            new Claim("thirdName", ThirdName),
            new Claim("role", ((int)Role).ToString()),
            new Claim("roleName", RoleString),
        };
    }

    public IEnumerable<KeyValuePair<string, object>> GetGeneratorClaims()
        => GetClaims().Select(x => new KeyValuePair<string, object>(x.Type, x.Value));

    public void ComputeHash()
        => PasswordHash = ComputeHash(Login, PasswordHash);
    public static string ComputeHash(string? login, string? password)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(login))
        {
            return string.Empty;
        }
        login = login.ToLower();
        StringBuilder rawBuilder = new StringBuilder();
        int counter = 0;
        int iteration = 0;
        while (rawBuilder.Length != login.Length + password.Length)
        {
            if (iteration % 2 == 0)
            {
                if (counter >= password.Length)
                {
                    rawBuilder.Append(login[counter]);
                }
                else
                {
                    rawBuilder.Append(password[counter]);
                }
            }
            else
            {
                if (counter >= login.Length)
                {
                    rawBuilder.Append(password[counter]);
                }
                else
                {
                    rawBuilder.Append(login[counter]);
                }
                counter++;
            }
            iteration++;
        }
        using (SHA384 hash = SHA384.Create())
        {
            byte[] bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(rawBuilder.ToString()));
            StringBuilder finalBuilder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                finalBuilder.Append(bytes[i].ToString("x2"));
            }
            return finalBuilder.ToString();
        }
    }

    public void Fire()
    {
        Fired = true;
        ResetRefreshToken();
    }

    public void Revive()
    {
        Fired = false;
        ResetRefreshToken();
    }

    public void ResetRefreshToken()
    {
        RefreshToken = null;
        RefreshExpiryTime = null;
    }

    public void RegenerateRefreshToken(int minutes)
    {
        RefreshToken = Guid.NewGuid().ToString();
        RefreshExpiryTime = DateTime.Now.AddMinutes(minutes);
    }
}