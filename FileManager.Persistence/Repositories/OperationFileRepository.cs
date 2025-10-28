using FileManager.Domain.Entities;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Persistence.Data;

namespace FileManager.Persistence.Repositories;

public class OperationFileRepository(FileManagerContext context) : IOperationFileRepository
{
    public async Task AddAsync(OperationFile operationFile)
    {
        await context.AddAsync(operationFile);
        await context.SaveChangesAsync();
    }
}
