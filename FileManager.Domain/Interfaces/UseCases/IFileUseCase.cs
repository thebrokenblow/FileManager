using FileManager.Domain.Model;

namespace FileManager.Domain.Interfaces.UseCases;

public interface IFileUseCase
{
    Task CreateFileAsync(FileModel fileModel, int userId);
    Task ModifyFileSizeOperationAsync(ModifyFileSizeOperationModel modifyFileSizeOperationModel);
}