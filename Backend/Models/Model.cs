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
public record struct CipherUserSettings(int RowCount, string SecretKey);

public sealed class AuthOptions
{
    public const string ISSUER = "MyCoolServerForCursach"; // издатель токена
    public const string AUDIENCE = "CURSACH_CLIENT"; // потребитель токена
    const string KEY = "verysecretimportantkeyamogushamstercombat"; // ключ для шифрации
    public static SymmetricSecurityKey GetSymmetricSecurityKey() => new(Encoding.UTF8.GetBytes(KEY));
}

public class ConfigurationString(IConfiguration configuration)
{
    public string ConnectionString => configuration.GetConnectionString("MyDB1") 
                                      ?? throw new InvalidOperationException("Строка подключения не может быть пустой");

    public string CipherConnectionString => configuration.GetConnectionString("MyDB2") 
                                            ?? throw new InvalidOperationException("Строка подключения не может быть пустой");
}

public record struct Message(int Id, string Text);
