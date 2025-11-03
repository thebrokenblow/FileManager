using FileManager.Domain.Entities;
using FileManager.Domain.Interfaces.Queries;
using FileManager.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace FileManager.Persistence.Queries;

public class DirectoryQueries(FileManagerContext context) : IDirectoryQueries
{
    private readonly FileManagerContext _context = context ??
        throw new ArgumentNullException(nameof(context));

    public async Task<InfoDirectory?> GetByLocationAsync(string fullPath)
    {
        var directoryOperations = await _context.DirectoryOperations
                                                .Where(operationDirectory => operationDirectory.Directory!.Location == fullPath)
                                                .OrderBy(operationDirectory => operationDirectory.ExecutedAt)
                                                .Select(operationDirectory => operationDirectory.Directory)
                                                .FirstOrDefaultAsync();

        return directoryOperations;
    }

    public async Task<int?> GetIdByLocationAsync(string fullPath)
    {
        var directoryId = await _context.DirectoryOperations
                                        .Where(operationDirectory => operationDirectory.Directory!.Location == fullPath)
                                        .OrderBy(operationDirectory => operationDirectory.ExecutedAt)
                                        .Select(operationDirectory => operationDirectory.DirectoryId)
                                        .FirstOrDefaultAsync();

        return directoryId;
    }
}