using System.Data;
using Microsoft.Data.Sqlite;

namespace Backend;

public interface IUserRepositoryFactory
{
    IUserRepository CreateUserRepository();
}
public class UserRepositoryFactory(string connectionString) : IUserRepositoryFactory
{
    public IUserRepository CreateUserRepository()
    {
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        return new UserRepository(dbConnection);
    }
}
public class UserRepository(IDbConnection dbConnection) : IUserRepository
{
    private readonly IDbConnection _dbConnection = dbConnection;
}
public interface IUserRepository
{
    void AddUser(User user);
    JsonContent GetUserHistory();
    void DeleteUserHistory();
    void ChangeUserPassword(string newPassword);
    
}