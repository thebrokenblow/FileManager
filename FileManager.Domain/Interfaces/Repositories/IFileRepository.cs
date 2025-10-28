using FileManager.Domain.Entities;

namespace FileManager.Domain.Interfaces.Repositories;

public interface IFileRepository
{
    Task AddAsync(InfoFile infoFile, OperationFile operationFile);
    Task UpdateAsync(InfoFile infoFile);
}