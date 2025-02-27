using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace WebApplication1.Controllers;

public class SqlCipherControllerFactory(ConfigurationString configurationString)
{
    public SqlCipherController CreateSqlCipherController()
    {
        return new SqlCipherController(new SqliteConnection(configurationString.CipherConnectionString));
    }
}

public class SqlCipherController(IDbConnection dbConnection)
{
    private readonly Cipher _cipher = new();

    public async Task<int> AddTextAsync(string text, string userLogin)
    {
        const string sql = "INSERT INTO Messages (UserLogin, Text) VALUES (@UserLogin, @Text);";
        const string sql2 =
            "SELECT MessageNumber FROM Messages WHERE UserLogin = @UserLogin ORDER BY MessageNumber DESC LIMIT 1;";
        await dbConnection.ExecuteAsync(sql, new { UserLogin = userLogin, Text = text });
        return await dbConnection.ExecuteScalarAsync<int>(sql2, new { UserLogin = userLogin });
    }

    public async Task<IEnumerable<Message>> GetTextAsync(string login)
    {
        const string sql = "SELECT MessageNumber, Text FROM Messages WHERE UserLogin = @UserLogin;";
        return await dbConnection.QueryAsync<Message>(sql, new { UserLogin = login });
    }

    public async Task<string?> GetTextAsync(int messageNumber, string login)
    {
        const string sql = "SELECT Text FROM Messages WHERE MessageNumber = @MessageNumber AND UserLogin = @UserLogin;";
        return await dbConnection.QuerySingleOrDefaultAsync<string>(sql,
            new { MessageNumber = messageNumber, UserLogin = login });
    }

    public async Task<bool> ChangeTextAsync(int messageNumber, string request, string login)
    {
        const string sql = "UPDATE Messages SET Text = @Text WHERE MessageNumber = @Id AND UserLogin = @UserLogin;";
        return await dbConnection.ExecuteAsync(sql, new { Id = messageNumber, Text = request, UserLogin = login }) != 0;
    }

    public async Task<int> DeleteTextAsync(int id, string login)
    {
        const string sql = "DELETE FROM Messages WHERE MessageNumber = @Id AND UserLogin = @UserLogin;";
        return await dbConnection.ExecuteAsync(sql, new { Id = id, UserLogin = login });
    }

    public async Task<bool> EncryptTextAsync(int id, string login, CipherUserSettings cipherUserSettings)
    {
        var text = await GetTextAsync(id, login);
        if (text == null) return false;
        _cipher.Keyword = cipherUserSettings.SecretKey;
        _cipher.Height = cipherUserSettings.RowCount;
        await ChangeTextAsync(id, _cipher.Encode(text), login);
        return true;
    }

    public async Task<bool> DecryptTextAsync(int id, string login, CipherUserSettings cipherUserSettings)
    {
        var text = await GetTextAsync(id, login);
        if (text == null) return false;
        _cipher.Keyword = cipherUserSettings.SecretKey;
        _cipher.Height = cipherUserSettings.RowCount;
        await ChangeTextAsync(id, _cipher.Decode(text), login);
        return true;
    }
}