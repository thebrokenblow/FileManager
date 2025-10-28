using FileManager.Domain.Entities;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Persistence.Data;

namespace FileManager.Persistence.Repositories;

public class DirectoryRepository(
    FileManagerContext context, 
    IOperationDirectoryRepository operationDirectoryRepository) : IDirectoryRepository
{
    public async Task AddAsync(InfoDirectory infoDirectory, OperationDirectory operationDirectory)
    {
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            await context.AddAsync(infoDirectory);
            await context.SaveChangesAsync();

            await operationDirectoryRepository.AddAsync(operationDirectory);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAsync(InfoDirectory infoDirectory)
    {
        context.Update(infoDirectory);
        await context.SaveChangesAsync();
    }
}