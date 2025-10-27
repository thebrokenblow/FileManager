using FileManager.Application.Actions.Interfaces;

namespace FileManager.Application.Actions.Disck;

public class InfoDisckAction : IAction
{
    public void Execute(string command)
    {
        var drives = DriveInfo.GetDrives();

        foreach (var drive in drives)
        {
            Console.WriteLine($"Название: {drive.Name}");
            Console.WriteLine($"Тип: {drive.DriveType}");
            Console.WriteLine($"Объем диска: {drive.TotalSize}");
            Console.WriteLine($"Свободное пространство: {drive.TotalFreeSpace}");
            Console.WriteLine($"Метка: {drive.VolumeLabel}");
        }
    }
}