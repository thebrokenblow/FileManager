using FileManager.Domain.Entities;
using FileManager.Domain.Entities.Enums;
using FileManager.Domain.Interfaces.Queries;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Persistence.Data;

namespace FileManager.Persistence.Repositories;

public class OperationFileRepository(FileManagerContext context, IFileQueries fileQueries) : IOperationFileRepository
{
    public async Task AddAsync(OperationFile operationFile)
    {
        await context.AddAsync(operationFile);
        await context.SaveChangesAsync();
    }

    public async Task RemoveAsync(string[] locationsFiles, DateTime dateTimeDelete, int userId)
    {
        foreach (var locationFile in locationsFiles)
        {
            var idDirectory = await fileQueries.GetIdByLocation(locationFile) ??
                    throw new ArgumentNullException(nameof(locationFile));

            var operationFile = new OperationFile
            {
                OperationType = OperationTypeFile.Delete,
                ExecutedAt = dateTimeDelete,
                FileId = idDirectory,
                UserId = userId
            };

            await context.AddAsync(operationFile);
        }

        await context.SaveChangesAsync();
    }
}