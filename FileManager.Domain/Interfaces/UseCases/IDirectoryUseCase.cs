using FileManager.Domain.Model;

namespace FileManager.Domain.Interfaces.UseCases;

public interface IDirectoryUseCase
{
    Task DeleteDirectoryAsync(DeleteDirectoryModel deleteDirectoryModel);
    Task MoveDirectoryAsync(MoveDirectoryModel moveDirectoryModel);
}