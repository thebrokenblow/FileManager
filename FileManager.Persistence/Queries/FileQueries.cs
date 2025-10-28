using FileManager.Domain.Entities;
using FileManager.Domain.Interfaces.Queries;
using FileManager.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace FileManager.Persistence.Queries;

public class FileQueries(FileManagerContext context) : IFileQueries
{
    public async Task<InfoFile?> GetByLocationAsync(string fullPath)
    {
        var fileOperation = await context.FileOperations
                                         .Where(operationFile => operationFile.File!.Location == fullPath)
                                         .OrderBy(operationFile => operationFile.ExecutedAt)
                                         .Select(operationFile => operationFile.File)
                                         .FirstOrDefaultAsync();

        return fileOperation;
    }

    public async Task<int?> GetIdByLocation(string fullPath)
    {
        var fileId = await context.FileOperations
                                  .Where(operationFile => operationFile.File!.Location == fullPath)
                                  .OrderBy(operationFile => operationFile.ExecutedAt)
                                  .Select(operationFile => operationFile.FileId)
                                  .FirstOrDefaultAsync();

        return fileId;
    }
}