using System.Data;
using Backend;
using Dapper;
using Microsoft.Data.Sqlite;

namespace WebApplication1.Controllers;

public class UserRepositoryFactory(ConfigurationString configuration)
{
    public UserRepository CreateUserRepository()
    {
        return new UserRepository(new SqliteConnection(configuration.ConnectionString));
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

    /// <summary>
    ///     Проверка логина и пароля
    /// </summary>
    /// <param name="user"></param>
    /// <returns>возвращает идентификатор пользователя</returns>
    public async Task<int> CheckUserAsync(User user)
    {
        const string sql = "SELECT user_id FROM user where login=@Login and password=@Password";
        return await dbConnection.QuerySingleOrDefaultAsync<int>(sql, user);
    }

    /// <summary>
    ///     проверка существования пользователя по логину
    /// </summary>
    /// <returns>возвращает идентификатор пользователя</returns>
    public async Task<int> GetUserAsync(string login)
    {
        const string sql = "SELECT user_id FROM user where login=@login";
        return await dbConnection.QuerySingleOrDefaultAsync<int>(sql, new { login });
    }

    public async Task AddUserHistoryAsync(UserHistory userHistory)
    {
        var userId = await GetUserAsync(userHistory.Login);
        userHistory.Login = userId.ToString();
        const string sql = "INSERT INTO user_history (user_id, QueryType, QueryDetails) "
                           + "VALUES (@Login, @QueryType, @QueryDetails)";
        await dbConnection.ExecuteAsync(sql, userHistory);
    }

    public async Task<IEnumerable<UserHistory>> GetUserHistoryAsync(int id)
    {
        const string sql = "SELECT u.login, uh.QueryType, uh.QueryDetails, uh.QueryTime "
                           + "FROM user_history uh JOIN user u ON uh.user_id = u.user_id WHERE u.user_id = @id";
        return await dbConnection.QueryAsync<UserHistory>(sql, new { id });
    }

    public async Task<int> DeleteUserHistoryAsync(string login)
    {
        const string sql = "DELETE FROM user_history WHERE user_id = (SELECT user_id FROM user WHERE login = @login)";
        return await dbConnection.ExecuteAsync(sql, new { login });
    }


    public async Task<bool> ChangeUserPasswordAsync(UserChangesPassword user)
    {
        const string sql = "UPDATE user SET Password = @NewPassword WHERE login = @Login and Password = @Password";
        return await dbConnection.ExecuteAsync(sql, user) == 1;
    }
}