using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Json;

namespace Client.Models;

public static class UserManager
{
    private const string DataFilePath = "user_data.json";
    public static event Action? UserDataChanged;

    public static User LoadUserData()
    {
        if (!File.Exists(DataFilePath)) return new User();
        var json = File.ReadAllText(DataFilePath);
        var user = JsonSerializer.Deserialize<User>(json) ?? new User();

        if (IsTokenExpired(user.JwtExpiry)) return user;
        user.Jwt = string.Empty;

        return user;
    }

    public static void SaveUserData(User data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(DataFilePath, json);
        UserDataChanged?.Invoke();
    }

    private static bool IsTokenExpired(DateTime expiry)
    {
        return expiry >= DateTime.UtcNow;
    }

    public static void DeleteUserData()
    {
        if (File.Exists(DataFilePath))
            File.Delete(DataFilePath);
    }

    public static void PrintUserData(User data)
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