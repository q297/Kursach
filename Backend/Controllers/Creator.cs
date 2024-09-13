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


public interface IUserRepository
{
    void AddUserAsync(User user);
    Task<IEnumerable<UserHistory>> GetUserHistoryAsync();
    void DeleteUserHistoryAsync();
    void ChangeUserPasswordAsync(string newPassword, string login);
}

public class UserRepository(IDbConnection dbConnection) : IUserRepository
{
    private readonly IDbConnection _dbConnection = dbConnection;

    public async void AddUserAsync(User user)
    {
        const string sql = "INSERT INTO Users (Login, Password) VALUES (@Login, @Password)";
        await _dbConnection.ExecuteAsync(sql, user);
    }

    public async void AddUserHistoryAsync(UserHistory userHistory)
    {
        const string sql = "INSERT INTO user_history (user_id, QueryType, QueryDetails, QueryTime) "
                           + "VALUES (@user_id, @QueryType, @QueryDetails, @QueryTime)";
        await _dbConnection.ExecuteAsync(sql, userHistory);
    }

    public async Task<IEnumerable<UserHistory>> GetUserHistoryAsync()
    {
        const string sql = "SELECT u.login, uh.QueryType, uh.QueryDetails, uh.QueryTime "
                           + "FROM user_history uh JOIN user u ON uh.user_id = u.user_id";
        return await _dbConnection.QueryAsync<UserHistory>(sql);
    }

    public async void DeleteUserHistoryAsync()
    {
        const string sql = "DELETE FROM user_history";
        await _dbConnection.ExecuteAsync(sql);
    }

    public async void ChangeUserPasswordAsync(string newPassword, string login)
    {
        const string sql = "UPDATE user SET Password = @newPassword WHERE login = @login";
        await _dbConnection.ExecuteAsync(sql, new { newPassword, login });
    }
}