using FileManager.Domain.Entities;
using FileManager.Domain.Interfaces.Queries;
using FileManager.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace FileManager.Persistence.Queries;

public class UserQueries(FileManagerContext context) : IUserQueries
{
    public async Task<int?> GetIdByLoginPasswordAsync(string login, string password, Func<string, string, bool> verifyPassword)
    {
        var user = await GetUserByLoginAsync(login);

        if (user is null)
        {
            return null;
        }

        if (verifyPassword.Invoke(password, user.PasswordHash))
        {
            return user.Id;
        }

        return null;
    }

    public async Task<bool> IsExistAsync(string login)
    {
        var isExist = await context.Users.AnyAsync(x => x.Username == login);

        return isExist;
    }

    private async Task<User?> GetUserByLoginAsync(string login)
    {
        var user = await context.Users
                            .Where(user => user.Username == login)
                            .FirstOrDefaultAsync();

        return user;
    }
}