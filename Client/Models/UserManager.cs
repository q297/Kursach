using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Json;

namespace Client.Models;

public class UserManager
{
    private const string DataFilePath = "user_data.json";

    public User LoadUserData()
    {
        if (File.Exists(DataFilePath))
        {
            var json = File.ReadAllText(DataFilePath);
            var user = JsonSerializer.Deserialize<User>(json) ?? new User();

            // Проверка срока действия токена
            if (!IsTokenExpired(user.Jwt)) return user;
            user.Jwt = string.Empty;

            return user;
        }

        return new User();
    }

    public void SaveUserData(User data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(DataFilePath, json);
    }

    private bool IsTokenExpired(string token)
    {
        if (string.IsNullOrEmpty(token))
            return true;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            return jwtToken.ValidTo < DateTime.UtcNow;
        }
        catch
        {
            return true;
        }
    }

    public void DeleteUserData()
    {
        if (File.Exists(DataFilePath))
            File.Delete(DataFilePath);
    }

    public void PrintUserData(User data)
    {
        var temp = (User)data.Clone();
        temp.Jwt = data.Jwt[..^70] + new string('*', 70);

        var userJson = JsonSerializer.Serialize(temp,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
        var user = new JsonText(userJson);
        AnsiConsole.Write(new Panel(user).Header("Профиль пользователя").BorderColor(Color.Aqua));
    }
}