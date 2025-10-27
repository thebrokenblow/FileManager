using FileManager.Data.Repositories;
using FileManager.Extensions;
using FileManager.Utils;
using System.IO.Compression;

namespace FileManager.Actions.FileAction;

public class ArchiveZipFilesAction(
    Menu menu, 
    FileRepository fileRepository) : IAction
{
    public void Execute(string command)
    {
        var arguments = command.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

        var nameCommand = arguments.First();
        if (!nameCommand.Equals(CommandDictionary.ArchiveZip, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException($"Команда: {command} не распознана");
        }

        var nameFileArchive = arguments.Second();
        if (string.IsNullOrWhiteSpace(nameFileArchive))
        {
            throw new ArgumentException("Название файла архива не может быть пустым");
        }

        if (PathSecurity.IsPathTraversal(nameFileArchive))
        {
            throw new ArgumentException($"Недопустимое имя файла архива: {nameFileArchive}");
        }

        if (!nameFileArchive.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            nameFileArchive += ".zip";
        }

        var filesToArchive = arguments[2..] ??
                                throw new ArgumentException("Файлы для архива не указаны");

        var fullPathFileArchive = Path.Combine(menu.CurrentPath, nameFileArchive);
        if (File.Exists(fullPathFileArchive))
        {
            throw new ArgumentException($"Архив с названием: {nameFileArchive} уже сеществует в данной директории");
        }

        using var archive = ZipFile.Open(fullPathFileArchive, ZipArchiveMode.Create);
        var files = new List<FileInfo>();
        foreach (var fileToArchive in filesToArchive)
        {
            var fullPathFileToArchive = Path.Combine(menu.CurrentPath, fileToArchive);

            if (!File.Exists(fullPathFileToArchive) && !Directory.Exists(fullPathFileToArchive))
            {
                throw new ArgumentException($"Не существует файла или директории: {fileToArchive} в текущей директории");
            }

            if (PathSecurity.IsPathTraversal(fileToArchive))
            {
                throw new ArgumentException($"Недопустимое имя: {fileToArchive}");
            }

            if (File.Exists(fullPathFileToArchive))
            {
                archive.CreateEntryFromFile(fullPathFileToArchive, fileToArchive);
            }
            else
            {
                AddDirectoryToArchive(archive, fullPathFileToArchive, fileToArchive);
            }
        }

        fileRepository.Create(menu.UserId, nameFileArchive, fullPathFileArchive);
    }

    private static void AddDirectoryToArchive(ZipArchive archive, string directoryPath, string relativePath)
    {
        var files = Directory.GetFiles(directoryPath);
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var entryName = Path.Combine(relativePath, fileName);

            archive.CreateEntryFromFile(file, entryName);
        }

        var subdirectories = Directory.GetDirectories(directoryPath);
        foreach (var subdirectory in subdirectories)
        {
            var dirName = Path.GetFileName(subdirectory);
            var newRelativePath = Path.Combine(relativePath, dirName);

            AddDirectoryToArchive(archive, subdirectory, newRelativePath);
        }
    }
}