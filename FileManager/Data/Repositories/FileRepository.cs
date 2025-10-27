using FileManager.Data.Entities;
using FileManager.Data.Entities.Enums;

namespace FileManager.Data.Repositories;

public class FileRepository(FileManagerContext context)
{
    public InfoFile? GetByLocation(string fullPath)
    {
        return context.FileOperations.Where(od => od.File!.Location == fullPath)
                                           .OrderBy(od => od.Timestamp)
                                           .Select(od => od.File)
                                           .FirstOrDefault();
    }

    public int GetIdByLocation(string fullPath)
    {
        var id = context.FileOperations.Where(od => od.File!.Location == fullPath)
                                            .OrderBy(od => od.Timestamp)
                                            .Select(od => od.FileId)
                                            .FirstOrDefault();

        return id;
    }

    public void Create(int userId, string nameFile, string fullPath)
    {
        using var transaction = context.Database.BeginTransaction();

        try
        {
            var timeCreateFile = DateTime.UtcNow;

            var infoFile = new InfoFile
            {
                Filename = nameFile,
                Location = fullPath,
                CreatedAt = timeCreateFile,
                UserId = userId,
            };
            context.Add(infoFile);

            var operationFile = new OperationFile
            {
                Timestamp = timeCreateFile,
                OperationType = OperationTypeFile.Create,
                FileId = infoFile.UserId,
                UserId = userId,
            };
            context.Add(operationFile);
            context.SaveChanges();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
        }
    }

    public void Update(InfoFile infoFile)
    {
        context.Update(infoFile);
        context.SaveChanges();
    }

    public void Delete(int userId, int fileId)
    {
        var timeDeleteFile = DateTime.UtcNow;

        try
        {
            var operationFile = new OperationFile
            {
                Timestamp = timeDeleteFile,
                OperationType = OperationTypeFile.Delete,
                UserId = userId,
                FileId = fileId,
            };

            context.Add(operationFile);
            context.SaveChanges();
        }
        catch
        {
            throw new Exception($"Ошибка при удалении директории");
        }
    }
}