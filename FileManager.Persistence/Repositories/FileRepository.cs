using FileManager.Domain.Entities;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Persistence.Data;

namespace FileManager.Persistence.Repositories;

public class FileRepository(
    FileManagerContext context, 
    IOperationFileRepository operationFileRepository) : IFileRepository
{
    public async Task AddAsync(InfoFile infoFile, OperationFile operationFile)
    {
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            await context.AddAsync(infoFile);
            await context.SaveChangesAsync();

            await operationFileRepository.AddAsync(operationFile);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAsync(InfoFile infoFile)
    {
        context.Update(infoFile);
        await context.SaveChangesAsync();
    }
}