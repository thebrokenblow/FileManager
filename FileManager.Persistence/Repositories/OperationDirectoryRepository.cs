using FileManager.Domain.Entities;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Persistence.Data;

namespace FileManager.Persistence.Repositories;

public class OperationDirectoryRepository(FileManagerContext context) : IOperationDirectoryRepository
{
    private readonly FileManagerContext _context = context ??
        throw new ArgumentNullException(nameof(context));

    public async Task AddAsync(OperationDirectory operationDirectory)
    {
        await _context.AddAsync(operationDirectory);
        await _context.SaveChangesAsync();
    }

    public async Task AddAsync(List<OperationDirectory> operationsDirectories)
    {
        await _context.AddRangeAsync(operationsDirectories);
        await _context.SaveChangesAsync();
    }
}