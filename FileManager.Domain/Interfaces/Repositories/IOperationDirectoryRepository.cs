using FileManager.Domain.Entities;

namespace FileManager.Domain.Interfaces.Repositories;

public interface IOperationDirectoryRepository
{
    Task AddAsync(OperationDirectory operationDirectory);
    Task AddAsync(List<OperationDirectory> operationsDirectories);
}