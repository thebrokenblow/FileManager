using FileManager.Domain.Entities;

namespace FileManager.Domain.Interfaces.Queries;

public interface IFileQueries
{
    Task<InfoFile?> GetByLocationAsync(string fullPath);
    Task<int?> GetIdByLocationAsync(string fullPath);
}