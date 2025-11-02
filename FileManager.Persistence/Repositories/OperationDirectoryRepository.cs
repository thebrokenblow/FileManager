using FileManager.Domain.Entities;
using FileManager.Domain.Entities.Enums;
using FileManager.Domain.Interfaces.Queries;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Persistence.Data;
using FileManager.Persistence.Queries;
using System.IO;

namespace FileManager.Persistence.Repositories;

public class OperationDirectoryRepository(FileManagerContext context, IDirectoryQueries directoryQueries) : IOperationDirectoryRepository
{
    public async Task AddAsync(OperationDirectory operationDirectory)
    {
        await context.AddAsync(operationDirectory);
        await context.SaveChangesAsync();
    }

    public async Task RemoveAsync(string[] locationsDirectories, DateTime dateTimeDelete, int userId)
    {
        foreach (var locationDirectory in locationsDirectories)
        {
            var idDirectory = await directoryQueries.GetIdByLocationAsync(locationDirectory) ?? 
                    throw new ArgumentNullException(nameof(locationDirectory));

            var operationDirectory = new OperationDirectory
            {
                OperationType = OperationTypeDirectory.Delete,
                ExecutedAt = dateTimeDelete,
                DirectoryId = idDirectory,
                UserId = userId
            };

            await context.AddAsync(operationDirectory);
        }

        await context.SaveChangesAsync();
    }
}