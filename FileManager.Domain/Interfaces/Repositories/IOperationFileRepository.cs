using FileManager.Domain.Entities;

namespace FileManager.Domain.Interfaces.Repositories;

public interface IOperationFileRepository
{
    Task AddAsync(OperationFile operationFile);
    Task RemoveAsync(string[] locationsFiles, DateTime dateTimeDelete, int userId);
}