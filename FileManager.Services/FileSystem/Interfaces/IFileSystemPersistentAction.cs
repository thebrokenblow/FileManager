namespace FileManager.Services.FileSystem.Interfaces;

public interface IFileSystemPersistentAction
{
    Task SaveToDatabaseAsync();
}