using FileManager.Domain.Entities;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Persistence.Data;

namespace FileManager.Persistence.Repositories;

public class OperationFileRepository(FileManagerContext context) : IOperationFileRepository
{
    private readonly FileManagerContext _context = context ??
        throw new ArgumentNullException(nameof(context));

    public async Task AddAsync(OperationFile operationFile)
    {
        await _context.AddAsync(operationFile);
        await _context.SaveChangesAsync();
    }

    public async Task AddAsync(List<OperationFile> operationFiles)
    {
        await _context.AddRangeAsync(operationFiles);
        await _context.SaveChangesAsync();
    }
}