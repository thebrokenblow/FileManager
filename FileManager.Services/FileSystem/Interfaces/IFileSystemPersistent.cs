namespace FileManager.Services.FileSystem.Interfaces;

public interface IFileSystemPersistent
{
    Task SaveToDatabaseAsync();
}