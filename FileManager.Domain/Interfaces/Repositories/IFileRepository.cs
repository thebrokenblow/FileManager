using FileManager.Domain.Entities;
using FileManager.Domain.Entities.Enums;

namespace FileManager.Domain.Interfaces.Repositories;

public interface IFileRepository
{
    Task AddAsync(InfoFile infoFile, OperationTypeFile operationTypeFile);
    Task UpdateAsync(InfoFile infoFile);
}