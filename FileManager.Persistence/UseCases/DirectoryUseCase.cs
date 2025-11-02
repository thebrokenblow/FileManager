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
    IOperationFileRepository operationFileRepository,
    IOperationDirectoryRepository operationDirectoryRepository) : IDirectoryUseCase
{
    public async Task DeleteDirectoryAsync(DeleteDirectoryModel deleteDirectoryModel)
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
                UserId = deleteDirectoryModel.UserId
            };

            await operationDirectoryRepository.AddAsync(operationDirectory);

            if (deleteDirectoryModel.ChildLocationsFiles is not null)
            {
                await operationFileRepository.RemoveAsync(
                    deleteDirectoryModel.ChildLocationsFiles,
                    deleteDirectoryModel.ExecutedAt,
                    deleteDirectoryModel.UserId);
            }

            if (deleteDirectoryModel.ChildLocationsDirectories is not null)
            {
                await operationDirectoryRepository.RemoveAsync(
                    deleteDirectoryModel.ChildLocationsDirectories,
                    deleteDirectoryModel.ExecutedAt,
                    deleteDirectoryModel.UserId);
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task MoveDirectoryAsync(MoveDirectoryModel moveDirectoryModel)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var infoDirectory = await directoryQueries.GetByLocationAsync(moveDirectoryModel.PathSourceDirectory) ??
                                        throw new Exception("Отсутствует соответствующая запись директории");

            infoDirectory.Location = moveDirectoryModel.NewFullPathDestinationDirectory;
            await directoryRepository.UpdateAsync(infoDirectory, moveDirectoryModel.UserId, moveDirectoryModel.ExecutedAt);

            var idSourceDirectory = await directoryQueries.GetIdByLocationAsync(moveDirectoryModel.PathDestinationDirectory) ??
                                            throw new Exception("Отсутствует соответствующая запись директории");

            var operationDestinationDirectory = new OperationDirectory
            {
                ExecutedAt = moveDirectoryModel.ExecutedAt,
                OperationType = OperationTypeDirectory.Modify,
                UserId = moveDirectoryModel.UserId,
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