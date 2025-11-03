using FileManager.Domain.Entities;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Persistence.Data;

namespace FileManager.Persistence.Repositories;

public class DirectoryRepository(FileManagerContext context) : IDirectoryRepository
{
    public async Task AddAsync(InfoDirectory infoDirectory)
    {
        await context.AddAsync(infoDirectory);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(InfoDirectory infoDirectory)
    {
        context.Update(infoDirectory);
        await context.SaveChangesAsync();
    }
}