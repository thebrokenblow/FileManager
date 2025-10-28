namespace FileManager.Application.FileSystem.Interfaces;

public interface IFileSystemPersistentAction
{
    Task SaveToDatabaseAsync();
}