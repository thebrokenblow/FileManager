using FileManager.Domain.Entities;
using FileManager.Domain.Interfaces.Queries;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Persistence.Data;

namespace FileManager.Persistence.Repositories;

public class OperationDirectoryRepository(FileManagerContext context, IDirectoryQueries directoryQueries) : IOperationDirectoryRepository
{
    public async Task AddAsync(OperationDirectory operationDirectory)
    {
        await context.AddAsync(operationDirectory);
        await context.SaveChangesAsync();
    }

    public async Task AddAsync(List<OperationDirectory> operationsDirectories)
    {
        await context.AddRangeAsync(operationsDirectories);
        await context.SaveChangesAsync();
    }
}