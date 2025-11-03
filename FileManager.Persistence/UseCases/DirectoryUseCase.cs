using FileManager.Domain.Entities;
using FileManager.Domain.Entities.Enums;
using FileManager.Domain.Interfaces.Queries;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Domain.Interfaces.UseCases;
using FileManager.Domain.Model;
using FileManager.Persistence.Data;

namespace FileManager.Persistence.UseCases;

public class DirectoryUseCase(
    FileManagerContext context,
    IDirectoryQueries directoryQueries,
    IDirectoryRepository directoryRepository,
    IOperationDirectoryRepository operationDirectoryRepository,
    IFileQueries fileQueries,
    IOperationFileRepository operationFileRepository) : IDirectoryUseCase
{
    public async Task CreateDirectoryAsync(DirectoryModel directoryModel, int userId)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var infoDirectory = new InfoDirectory
            {
                DirectoryName = directoryModel.DirectoryName,
                CreatedAt = directoryModel.CreatedAt,
                Location = directoryModel.Location,
                UserId = userId
            };

            await directoryRepository.AddAsync(infoDirectory);

            var operationDirectory = new OperationDirectory
            {
                ExecutedAt = directoryModel.CreatedAt,
                OperationType = OperationTypeDirectory.Create,
                DirectoryId = infoDirectory.Id,
                UserId = userId,
            };

            await operationDirectoryRepository.AddAsync(operationDirectory);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteDirectoryAsync(DeleteDirectoryModel deleteDirectoryModel, int userId)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var directoryId = await directoryQueries.GetIdByLocationAsync(deleteDirectoryModel.PathDirectory) ??
                                        throw new InvalidOperationException("Некорректное поведение системы, проблема с базой данных");

            var operationDirectory = new OperationDirectory
            {
                OperationType = OperationTypeDirectory.Delete,
                ExecutedAt = deleteDirectoryModel.ExecutedAt,
                DirectoryId = directoryId,
                UserId = userId
            };

            await operationDirectoryRepository.AddAsync(operationDirectory);

            if (deleteDirectoryModel.ChildLocationsFiles is not null)
            {
                await SaveRangeOperationsFilesAsync(
                    deleteDirectoryModel.ChildLocationsFiles, 
                    deleteDirectoryModel.ExecutedAt, 
                    userId);
            }

            if (deleteDirectoryModel.ChildLocationsDirectories is not null)
            {
                await SaveRangeOperationsDirectoriesAsync(
                    deleteDirectoryModel.ChildLocationsDirectories,
                    deleteDirectoryModel.ExecutedAt,
                    userId);
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task SaveRangeOperationsFilesAsync(string[] locationsFiles, DateTime executedAt, int userId)
    {
        var operationsFiles = new List<OperationFile>();

        foreach (var locationFile in locationsFiles)
        {
            var fileId = await fileQueries.GetIdByLocationAsync(locationFile) ??
                                throw new InvalidOperationException("Некорректное поведение системы, проблема с базой данных");

            var operationFile = new OperationFile
            {
                OperationType = OperationTypeFile.Delete,
                ExecutedAt = executedAt,
                FileId = fileId,
                UserId = userId
            };

            operationsFiles.Add(operationFile);
        }

        await operationFileRepository.AddAsync(operationsFiles);
    }

    public async Task SaveRangeOperationsDirectoriesAsync(string[] locationsDirectories, DateTime executedAt, int userId)
    {
        var operationsDirectories = new List<OperationDirectory>();

        foreach (var locationDirectory in locationsDirectories)
        {
            var directoryId = await directoryQueries.GetIdByLocationAsync(locationDirectory) ??
                                        throw new InvalidOperationException("Некорректное поведение системы, проблема с базой данных");

            var operationDirectory = new OperationDirectory
            {
                OperationType = OperationTypeDirectory.Delete,
                ExecutedAt = executedAt,
                DirectoryId = directoryId,
                UserId = userId
            };

            operationsDirectories.Add(operationDirectory);
        }

        await operationDirectoryRepository.AddAsync(operationsDirectories);
    }

    public async Task MoveDirectoryAsync(MoveDirectoryModel moveDirectoryModel, int userId)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var infoDirectory = await directoryQueries.GetByLocationAsync(moveDirectoryModel.PathSourceDirectory) ??
                                        throw new Exception("Отсутствует соответствующая запись директории");

            infoDirectory.UserId = userId;
            infoDirectory.Location = moveDirectoryModel.NewFullPathDestinationDirectory;

            await directoryRepository.UpdateAsync(infoDirectory);

            var idSourceDirectory = await directoryQueries.GetIdByLocationAsync(moveDirectoryModel.PathDestinationDirectory) ??
                                            throw new Exception("Отсутствует соответствующая запись директории");

            var operationDestinationDirectory = new OperationDirectory
            {
                ExecutedAt = moveDirectoryModel.ExecutedAt,
                OperationType = OperationTypeDirectory.Modify,
                UserId = userId,
                DirectoryId = idSourceDirectory
            };

            await operationDirectoryRepository.AddAsync(operationDestinationDirectory);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}