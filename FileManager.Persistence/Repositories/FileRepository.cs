using FileManager.Domain.Entities;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Persistence.Data;

namespace FileManager.Persistence.Repositories;

public class FileRepository(FileManagerContext context) : IFileRepository
{
    private readonly FileManagerContext _context = context ??
        throw new ArgumentNullException(nameof(context));

    public async Task AddAsync(InfoFile infoFile)
    {
        await _context.AddAsync(infoFile);
        await _context.SaveChangesAsync();
    }

    public async Task AddAsync(List<InfoFile> infoFile)
    {
        await _context.AddRangeAsync(infoFile);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(InfoFile infoFile)
    {
        _context.Update(infoFile);
        await _context.SaveChangesAsync();
    }
}