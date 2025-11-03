using FileManager.Domain.Entities;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Persistence.Data;

namespace FileManager.Persistence.Repositories;

public class FileRepository(FileManagerContext context) : IFileRepository
{
    public async Task AddAsync(InfoFile infoFile)
    {
        await context.AddAsync(infoFile);
        await context.SaveChangesAsync();
    }

    public async Task AddAsync(List<InfoFile> infoFile)
    {
        await context.AddRangeAsync(infoFile);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(InfoFile infoFile)
    {
        context.Update(infoFile);
        await context.SaveChangesAsync();
    }
}