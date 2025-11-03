using FileManager.Domain.Entities;

namespace FileManager.Domain.Interfaces.Repositories;

public interface IOperationFileRepository
{
    Task AddAsync(OperationFile operationFile);
    Task AddAsync(List<OperationFile> operationFiles);
}