using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Backend;

public record struct User([Required] string Login, [Required] string Password);

public record struct UserHistory(
    string Login,
    string QueryType,
    string? QueryDetails);

public record struct UserChangesPassword(
    [Required] string Login,
    [Required] string Password,
    [Required] string NewPassword);

public sealed class AuthOptions
{
    public const string ISSUER = "MyCoolServerForCursach"; // издатель токена
    public const string AUDIENCE = "CURSACH_CLIENT"; // потребитель токена
    const string KEY = "verysecretimportantkeyamogushamstercombat"; // ключ для шифрации
    public static SymmetricSecurityKey GetSymmetricSecurityKey() => new(Encoding.UTF8.GetBytes(KEY));
}