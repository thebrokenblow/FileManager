namespace FileManager.Services.FileSystem.Interfaces;

public interface IFileSystemCommand
{
    /// <summary>
    /// Выполнить команду файлового менеджера
    /// </summary>
    void Execute(string command);
}