using FileManager.Domain.Entities;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Persistence.Data;

namespace FileManager.Persistence.Repositories;

public class UserRepository(FileManagerContext context) : IUserRepository
{
    public async Task<int> AddAsync(User user)
    {
        await context.AddAsync(user);
        await context.SaveChangesAsync();

        return user.Id;
    }
}