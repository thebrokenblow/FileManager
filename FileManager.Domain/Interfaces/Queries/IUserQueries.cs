namespace FileManager.Domain.Interfaces.Queries;

public interface IUserQueries
{
    Task<int?> GetIdByLoginPasswordAsync(string login, string password, Func<string, string, bool> verifyPassword);
    Task<bool> IsExistAsync(string login);
}