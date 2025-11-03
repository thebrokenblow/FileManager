using FileManager.Domain.Model;

namespace FileManager.Domain.Interfaces.UseCases;

public interface IDirectoryUseCase
{
    Task CreateAsync(DirectoryModel directoryModel, int userId);
    Task DeleteAsync(DeleteDirectoryModel deleteDirectoryModel, int userId);
    Task MoveAsync(MoveDirectoryModel moveDirectoryModel, int userId);
}