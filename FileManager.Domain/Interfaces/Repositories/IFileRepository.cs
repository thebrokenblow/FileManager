using FileManager.Domain.Entities;

namespace FileManager.Domain.Interfaces.Repositories;

public interface IFileRepository
{
    Task AddAsync(InfoFile infoFile);
    Task AddAsync(List<InfoFile> infoFile);
    Task UpdateAsync(InfoFile infoFile);
}