using System.Data;
using Backend;
using Dapper;
using Microsoft.Data.Sqlite;

namespace WebApplication1.Controllers;

public static class UserRepositoryFactory
{
    private static readonly IConfiguration _configuration =
        new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

    public static UserRepository CreateUserRepository()
    {
        var connectionString = _configuration.GetConnectionString("MyDb1");
        return new UserRepository(new SqliteConnection(connectionString));
    }
}

public class UserRepository(IDbConnection dbConnection)
{
    public async Task<int> AddUserAsync(User user)
    {
        const string sql = "INSERT INTO user (Login, Password) VALUES (@Login, @Password); SELECT last_insert_rowid();";

        var userId = await dbConnection.ExecuteScalarAsync<int>(sql, user);

        return userId;
    }


    public async void AddUserHistoryAsync(UserHistory userHistory)
    {
        const string sql = "INSERT INTO user_history (user_id, QueryType, QueryDetails, QueryTime) "
                           + "VALUES (@user_id, @QueryType, @QueryDetails, @QueryTime)";
        await dbConnection.ExecuteAsync(sql, userHistory);
    }

    public async Task<IEnumerable<UserHistory>> GetUserHistoryAsync(string id)
    {
        const string sql = "SELECT u.login, uh.QueryType, uh.QueryDetails, uh.QueryTime "
                           + "FROM user_history uh JOIN user u ON uh.user_id = u.user_id WHERE u.user_id = @id";
        return await dbConnection.QueryAsync<UserHistory>(sql, new { id });
    }

    public async Task<int> DeleteUserHistoryAsync(string id)
    {
        const string sql = "DELETE FROM user_history WHERE user_id = @id ";
        return await dbConnection.ExecuteAsync(sql, new { id });
    }

    public async Task ChangeUserPasswordAsync(string newPassword, string login)
    {
        const string sql = "UPDATE user SET Password = @newPassword WHERE login = @login";
        await dbConnection.ExecuteAsync(sql, new { newPassword, login });
    }
}