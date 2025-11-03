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
    private readonly FileManagerContext _context = context ??
        throw new ArgumentNullException(nameof(context));

    private readonly IFileQueries _fileQueries = fileQueries ??
        throw new ArgumentNullException(nameof(fileQueries));

    private readonly IFileRepository _fileRepository = fileRepository ??
        throw new ArgumentNullException(nameof(fileRepository));

    private readonly IOperationFileRepository _operationFileRepository = operationFileRepository ??
        throw new ArgumentNullException(nameof(operationFileRepository));

    public async Task CreateAsync(FileModel fileModel, int userId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

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

            await _fileRepository.AddAsync(infoFile);

            var operationFile = new OperationFile
            {
                ExecutedAt = fileModel.CreateAt,
                OperationType = OperationTypeFile.Create,
                FileId = infoFile.Id,
                UserId = userId
            };

            await _operationFileRepository.AddAsync(operationFile);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ModifySizeAsync(WriteFileModel modifyFileSizeOperationModel)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var file = await _fileQueries.GetByLocationAsync(modifyFileSizeOperationModel.PathFile) ??
                                throw new Exception();

            file.Size = modifyFileSizeOperationModel.FileSize;
            await _fileRepository.UpdateAsync(file);

            var operationFile = new OperationFile
            {
                OperationType = OperationTypeFile.Modify,
                ExecutedAt = modifyFileSizeOperationModel.ExecutedAt,
                FileId = file.Id,
                UserId = modifyFileSizeOperationModel.UserId,
            };

            await _operationFileRepository.AddAsync(operationFile);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task MoveAsync(MoveFileModel moveFileModel, int userId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var infoFile = await _fileQueries.GetByLocationAsync(moveFileModel.PathSourceFile) ??
                                    throw new Exception($"Файл не найден в базе данных: {moveFileModel.PathSourceFile}", null);

            infoFile.Location = moveFileModel.NewPathSourceFile;
            await _fileRepository.UpdateAsync(infoFile);

            var operationFile = new OperationFile
            {
                OperationType = OperationTypeFile.Modify,
                ExecutedAt = moveFileModel.ExecuteAt,
                FileId = infoFile.Id,
                UserId = userId
            };

            await _operationFileRepository.AddAsync(operationFile);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}