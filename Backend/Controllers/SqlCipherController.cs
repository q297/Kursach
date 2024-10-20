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
    private Cipher _cipher = new Cipher();
    public async Task AddTextAsync(string text, string userLogin)
    {
        const string sql = "INSERT INTO Messages (UserLogin, Text) VALUES (@UserLogin, @Text);";
        await dbConnection.ExecuteAsync(sql, new { UserLogin = userLogin, Text = text });
    }

    public async Task<IEnumerable<string>> GetTextAsync(string login)
    {
        const string sql1 = "SELECT Text FROM Messages WHERE UserLogin = @UserLogin;";
        return await dbConnection.QueryAsync<string>(sql1, new { UserLogin = login });
    }

    public async Task<string?> GetTextAsync(int id, string login)
    {
        const string sql = "SELECT Text FROM Messages WHERE Id = @Id AND UserLogin = @UserLogin;";
        return await dbConnection.QuerySingleOrDefaultAsync<string>(sql, new { Id = id, UserLogin = login });
    }

    public async Task ChangeTextAsync(int id, string request, string login)
    {
        const string sql = "UPDATE Messages SET Text = @Text WHERE Id = @Id AND UserLogin = @UserLogin;";
        await dbConnection.ExecuteAsync(sql, new {Id = id, Text = request, UserLogin = login });
    }

    public async Task DeleteTextAsync(int id, string login)
    {
        const string sql = "DELETE FROM Messages WHERE Id = @Id AND UserLogin = @UserLogin;";
        await dbConnection.ExecuteAsync(sql, new { Id = id, UserLogin = login });
    }

    public async Task EncryptTextAsync(int id, string login, CipherUserSettings cipherUserSettings)
    {
        var text = await GetTextAsync(id, login);
        if (text != null)
        {
            _cipher.SecretKey = cipherUserSettings.SecretKey;
            _cipher.Text = text;
            _cipher.RowCount = cipherUserSettings.RowCount;
        }
        await ChangeTextAsync(id, _cipher.Encrypt(), login);
    }

    public async Task DecryptTextAsync(int id, string login, CipherUserSettings cipherUserSettings)
    {
        var text = await GetTextAsync(id, login);
        if (text != null)
        {
            _cipher.SecretKey = cipherUserSettings.SecretKey;
            _cipher.Text = text;
            _cipher.RowCount = cipherUserSettings.RowCount;
        }
        await ChangeTextAsync(id, _cipher.Decrypt(), login);
    }
}