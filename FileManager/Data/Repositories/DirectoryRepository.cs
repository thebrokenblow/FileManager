using FileManager.Data.Entities;
using FileManager.Data.Entities.Enums;

namespace FileManager.Data.Repositories;

public class DirectoryRepository(
    FileManagerContext context, 
    OperationDirectoryRepository operationDirectoryRepository)
{
    public void Update(InfoDirectory infoDirectory)
    {
        context.Update(infoDirectory);
        context.SaveChanges();
    }

    public InfoDirectory? GetByLocation(string fullPath)
    {
        return context.DirectoryOperations.Where(od => od.Directory!.Location == fullPath)
                                           .OrderBy(od => od.Timestamp)
                                           .Select(od => od.Directory)
                                           .FirstOrDefault();
    }

    public int GetIdByLocation(string fullPath)
    {
        var id = context.DirectoryOperations.Where(od => od.Directory!.Location == fullPath)
                                            .OrderBy(od => od.Timestamp)
                                            .Select(od => od.DirectoryId) 
                                            .FirstOrDefault();

        return id;
    }

    public void Delete(int userId, int directoryId)
    {
        var timeDeleteDirectory = DateTime.UtcNow;

        try
        {
            var operationDirectory = new OperationDirectory
            {
                UserId = userId,
                DirectoryId = directoryId,
                OperationType = OperationTypeDirectory.Delete,
                Timestamp = timeDeleteDirectory,
            };

            context.Add(operationDirectory);
            context.SaveChanges();
        }
        catch
        {
            throw new Exception($"Ошибка при удалении директории");
        }
    }

    public async Task Create(int userId, string nameDirectory, string fullPathDirectory)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        var timeCreateDirectory = DateTime.UtcNow;

        try
        {
            var infoDirectory = new InfoDirectory
            {
                DirectoryName = nameDirectory,
                CreatedAt = timeCreateDirectory,
                Location = fullPathDirectory,
                UserId = userId,
            };

            await context.AddAsync(infoDirectory);
            await context.SaveChangesAsync();

            await operationDirectoryRepository.AddAsync(userId, infoDirectory.Id, timeCreateDirectory);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();

            throw new Exception($"Ошибка записи данных в бд");
        }
    }
}