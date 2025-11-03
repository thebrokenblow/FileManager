using FileManager.Domain.Entities;
using FileManager.Domain.Model;

namespace FileManager.Domain.Interfaces.UseCases;

public interface IDirectoryUseCase
{
    Task CreateDirectoryAsync(DirectoryModel directoryModel, int userId);
    Task DeleteDirectoryAsync(DeleteDirectoryModel deleteDirectoryModel, int userId);
    Task MoveDirectoryAsync(MoveDirectoryModel moveDirectoryModel, int userId);
}