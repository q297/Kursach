using System.Text.Json.Serialization;

namespace Client.Models;

public sealed class User : ICloneable
{
    [JsonPropertyName("userId")] public int UserId { get; set; }

    [JsonPropertyName("login")] public string? Login { get; set; }

    [JsonIgnore] public string? Password { get; set; }

    [JsonPropertyName("token")] public string Jwt { get; set; } = null!;

    [JsonPropertyName("expiry")] public DateTime JwtExpiry { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}

public class RegisterResponse
{
    [JsonPropertyName("message")] public string Message { get; set; }

    [JsonPropertyName("userId")] public int UserId { get; set; }

    [JsonPropertyName("token")] public string Token { get; set; }

    [JsonPropertyName("expiry")] public DateTime Expiry { get; set; }
}

public class PatchPasswordResponse
{
    [JsonPropertyName("message")] public string Message { get; set; }
    public string Token { get; set; }
}

public sealed class UserHistoryResponseWrapper
{
    public IEnumerable<UserHistoryResponse> UserHistories { get; set; } = Enumerable.Empty<UserHistoryResponse>();
}

public sealed class UserHistoryResponse
{
    public string Login { get; set; }
    public string QueryType { get; set; }
    public string? QueryDetails { get; set; } = "Описание отсутствует";
}

public sealed class MessagesResponse
{
    public int MessageNumber { get; set; }
    public string Text { get; set; }
}

public sealed class CipherUserSettings
{
    public int RowCount { get; set; }
    public string SecretKey { get; set; }
}

public class Settings
{
    public int RowCount { get; set; }
    public string SecretKey { get; set; } = string.Empty;
}