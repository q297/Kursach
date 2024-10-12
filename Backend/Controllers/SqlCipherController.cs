using System.Data;

namespace WebApplication1.Controllers;

public class SqlCipherController(IDbConnection dbConnection)
{
    public async Task AddTextAsync(string request, string login)
    {
        throw new NotImplementedException();
    }

    public async Task<string?> GetTextAsync(int? id, string login)
    {
        throw new NotImplementedException();
    }

    public async Task ChangeTextAsync(int id, string request, string login)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteTextAsync(int id, string login)
    {
        throw new NotImplementedException();
    }

    public async Task EncryptTextAsync(int id, string login)
    {
        throw new NotImplementedException();
    }

    public async Task DecryptTextAsync(int id, string login)
    {
        throw new NotImplementedException();
    }
}