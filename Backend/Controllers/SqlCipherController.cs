using System.Data;
using Backend;
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
    private readonly Crypter _crypter = new();

    public async Task AddTextAsync(string text, string userLogin)
    {
        const string sql = "INSERT INTO Messages (UserLogin, Text) VALUES (@UserLogin, @Text);";
        await dbConnection.ExecuteAsync(sql, new { UserLogin = userLogin, Text = text });
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

    public async Task ChangeTextAsync(int id, string request, string login)
    {
        const string sql = "UPDATE Messages SET Text = @Text WHERE MessageNumber = @Id AND UserLogin = @UserLogin;";
        await dbConnection.ExecuteAsync(sql, new { Id = id, Text = request, UserLogin = login });
    }

    public async Task DeleteTextAsync(int id, string login)
    {
        const string sql = "DELETE FROM Messages WHERE Id = @Id AND UserLogin = @UserLogin;";
        await dbConnection.ExecuteAsync(sql, new { Id = id, UserLogin = login });
    }

    public async Task<bool> EncryptTextAsync(int id, string login, CipherUserSettings cipherUserSettings)
    {
        var text = await GetTextAsync(id, login);
        if (text == null) return false;
        _crypter.Keyword = cipherUserSettings.SecretKey;
        _crypter.Height = cipherUserSettings.RowCount;
        await ChangeTextAsync(id, _crypter.Encode(text), login);
        return true;
    }

    public async Task<bool> DecryptTextAsync(int id, string login, CipherUserSettings cipherUserSettings)
    {
        var text = await GetTextAsync(id, login);
        if (text == null) return false;
        _crypter.Keyword = cipherUserSettings.SecretKey;
        _crypter.Height = cipherUserSettings.RowCount;
        await ChangeTextAsync(id, _crypter.Decode(text), login);
        return true;
    }
}