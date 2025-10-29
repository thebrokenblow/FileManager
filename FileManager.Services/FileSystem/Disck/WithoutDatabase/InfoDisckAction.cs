using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;
using System.Text;

namespace FileManager.Services.FileSystem.Disck.WithoutDatabase;

public class InfoDisckAction(IMenu menu) : IFileSystemAction
{
    private readonly IMenu _menu = menu ?? throw new ArgumentNullException(nameof(menu));

    private readonly CommandValidator commandValidator = new();
    public void Execute(string command)
    {
        commandValidator
            .ValidateNotEmpty(command, "Команда не может быть пустой")
            .ValidateCommandName(command, CommandDictionary.ShowDiskInfo, $"Команда не распознана: {command}");

        var drives = DriveInfo.GetDrives();

        var stringResult = new StringBuilder();

        foreach (var drive in drives)
        {
            stringResult.AppendLine($"Название: {drive.Name}");
            stringResult.AppendLine($"Тип: {drive.DriveType}");
            stringResult.AppendLine($"Объем диска: {drive.TotalSize}");
            stringResult.AppendLine($"Свободное пространство: {drive.TotalFreeSpace}");
        }

        _menu.Output.Invoke(stringResult.ToString());
    }
}