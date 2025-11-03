using FileManager.Domain.Model;

namespace FileManager.Domain.Interfaces.UseCases;

public interface IFileUseCase
{
    Task CreateAsync(FileModel fileModel, int userId);
    Task MoveAsync(MoveFileModel moveFileModel, int userId);
    Task ModifySizeAsync(WriteFileModel modifyFileSizeOperationModel);
}