using FileManager.Domain.Entities;

namespace FileManager.Domain.Interfaces.Queries;

public interface IFileQueries
{
    Task<int?> GetIdByLocationAsync(string fullPath);
    Task<InfoFile?> GetByLocationAsync(string fullPath);
}