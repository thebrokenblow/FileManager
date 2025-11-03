using FileManager.Domain.Entities;
using FileManager.Domain.Entities.Enums;
using FileManager.Domain.Interfaces.Queries;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Domain.Interfaces.UseCases;
using FileManager.Domain.Model;
using FileManager.Persistence.Data;

namespace FileManager.Persistence.UseCases;

public class FileUseCase(
    FileManagerContext context,
    IFileQueries fileQueries, 
    IFileRepository fileRepository, 
    IOperationFileRepository operationFileRepository) : IFileUseCase
{
    public async Task CreateFileAsync(FileModel fileModel, int userId)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var infoFile = new InfoFile
            {
                Filename = fileModel.FileName,
                Location = fileModel.FullPath,
                CreatedAt = fileModel.CreateAt,
                Size = fileModel.Size,
                UserId = userId
            };

            await fileRepository.AddAsync(infoFile);

            var operationFile = new OperationFile
            {
                ExecutedAt = fileModel.CreateAt,
                OperationType = OperationTypeFile.Create,
                FileId = infoFile.Id,
                UserId = userId
            };

            await operationFileRepository.AddAsync(operationFile);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ModifyFileSizeOperationAsync(ModifyFileSizeOperationModel modifyFileSizeOperationModel)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var file = await fileQueries.GetByLocationAsync(modifyFileSizeOperationModel.PathFile) ??
                                throw new Exception();

            file.Size = modifyFileSizeOperationModel.FileSize;
            await fileRepository.UpdateAsync(file);

            var operationFile = new OperationFile
            {
                OperationType = OperationTypeFile.Modify,
                ExecutedAt = modifyFileSizeOperationModel.ExecutedAt,
                FileId = file.Id,
                UserId = modifyFileSizeOperationModel.UserId,
            };

            await operationFileRepository.AddAsync(operationFile);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}