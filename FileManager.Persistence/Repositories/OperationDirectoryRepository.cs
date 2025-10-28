using FileManager.Domain.Entities;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Persistence.Data;

namespace FileManager.Persistence.Repositories;

public class OperationDirectoryRepository(FileManagerContext context) : IOperationDirectoryRepository
{
    public async Task AddAsync(OperationDirectory operationDirectory)
    {
        await context.AddAsync(operationDirectory);
        await context.SaveChangesAsync();
    }
}