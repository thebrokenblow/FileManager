using FileManager.Domain.Entities;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Persistence.Data;

namespace FileManager.Persistence.Repositories;

public class DirectoryRepository(FileManagerContext context) : IDirectoryRepository
{
    private readonly FileManagerContext _context = context ??
        throw new ArgumentNullException(nameof(context));

    public async Task AddAsync(InfoDirectory infoDirectory)
    {
        await _context.AddAsync(infoDirectory);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(InfoDirectory infoDirectory)
    {
        _context.Update(infoDirectory);
        await _context.SaveChangesAsync();
    }
}