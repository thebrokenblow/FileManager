using FileManager.Domain.Entities;

namespace FileManager.Domain.Interfaces.Queries;

public interface IDirectoryQueries
{
    Task<InfoDirectory?> GetByLocationAsync(string fullPath);
    Task<int?> GetIdByLocationAsync(string fullPath);
}