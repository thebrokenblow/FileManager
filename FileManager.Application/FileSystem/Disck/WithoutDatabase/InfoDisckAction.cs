using FileManager.Application.FileSystem.Interfaces;
using System.Text;

namespace FileManager.Application.FileSystem.Disck.WithoutDatabase;

public class InfoDisckAction(IMenu menu) : IFileSystemAction
{
    private readonly IMenu _menu = menu ?? throw new ArgumentNullException(nameof(menu));

    public void Execute(string command)
    {
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