using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public record struct UserHistory(
    string Login,
    string QueryType,
    string? QueryDetails);

public sealed class User
{
    [Required] public string Login { get; set; }

    [Required] public string Password { get; set; }
}

public sealed class UserChangesPassword
{
    [Required] public string Login { get; set; }

    [Required] public string Password { get; set; }

    [Required] public string NewPassword { get; set; }
}

public sealed class CipherUserSettings
{
    [Range(1, 50, ErrorMessage = "RowCount must be greater than zero and below than 50.")]
    public int RowCount { get; set; }

    [Required(ErrorMessage = "SecretKey is required.")]
    [MinLength(1, ErrorMessage = "SecretKey cannot be empty.")]
    public string SecretKey { get; set; }
}

public sealed class AuthOptions
{
    public const string Issuer = "MyCoolServerForCursach"; // издатель токена
    public const string Audience = "CURSACH_CLIENT"; // потребитель токена
    private const string Key = "verysecretimportantkeyamogushamstercombat"; // ключ для шифрации

    public static SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
    }
}

public class ConfigurationString(IConfiguration configuration)
{
    public string ConnectionString => configuration.GetConnectionString("MyDB1")
                                      ?? throw new InvalidOperationException("Строка подключения не может быть пустой");

    public string CipherConnectionString => configuration.GetConnectionString("MyDB2")
                                            ?? throw new InvalidOperationException(
                                                "Строка подключения не может быть пустой");
}

public record struct Message(int MessageNumber, string Text);