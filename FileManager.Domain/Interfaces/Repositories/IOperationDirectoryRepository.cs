using FileManager.Domain.Entities;

namespace FileManager.Domain.Interfaces.Repositories;

public interface IOperationDirectoryRepository
{
    Task AddAsync(OperationDirectory operationDirectory);
    Task RemoveAsync(string[] locationsDirectories, DateTime dateTimeDelete, int userId);
}