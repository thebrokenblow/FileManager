using FileManager.Data.Entities;

namespace FileManager.Data.Repositories;

public class OperationDirectoryRepository(FileManagerContext context)
{
    public async Task AddAsync(OperationDirectory operationDirectory)
    {
        await context.AddAsync(operationDirectory);
        await context.SaveChangesAsync();
    }
}