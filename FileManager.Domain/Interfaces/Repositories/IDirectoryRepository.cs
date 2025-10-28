using FileManager.Domain.Entities;

namespace FileManager.Domain.Interfaces.Repositories;

public interface IDirectoryRepository
{
    Task AddAsync(InfoDirectory infoDirectory, OperationDirectory operationDirectory);
    Task UpdateAsync(InfoDirectory infoDirectory);
}