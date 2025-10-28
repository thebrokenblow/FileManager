using FileManager.Application.Extensions;
using FileManager.Application.FileSystem.Interfaces;
using FileManager.Application.Utils;
using FileManager.Domain.Entities;
using FileManager.Domain.Entities.Enums;
using FileManager.Domain.Interfaces.Repositories;
using System.IO.Compression;

namespace FileManager.Application.FileSystem.FileAction.Persistent;

public class ArchiveZipFilesAction(
    IMenu menu,
    IFileRepository fileRepository) : IFileSystemAction, IFileSystemPersistentAction
{
    private readonly IMenu _menu = menu ?? 
        throw new ArgumentNullException(nameof(menu));

    private readonly IFileRepository _fileRepository = fileRepository ?? 
        throw new ArgumentNullException(nameof(fileRepository));

    private string? _nameFileArchive;
    private string? _fullPathFileArchive;

    public void Execute(string command)
    {
        var arguments = command.Split(WhitespaceCharsDictionary.AllWhitespace, StringSplitOptions.RemoveEmptyEntries);

        var nameCommand = arguments.First();
        if (!nameCommand.Equals(CommandDictionary.ArchiveZip, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException($"Команда: {command} не распознана");
        }

        _nameFileArchive = arguments.Second();
        if (string.IsNullOrWhiteSpace(_nameFileArchive))
        {
            throw new ArgumentException("Название файла архива не может быть пустым");
        }

        if (PathSecurity.IsPathTraversal(_nameFileArchive))
        {
            throw new ArgumentException($"Недопустимое имя файла архива: {_nameFileArchive}");
        }

        if (!_nameFileArchive.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            _nameFileArchive += ".zip";
        }

        var filesToArchive = arguments[2..] ??
                                throw new ArgumentException("Файлы для архива не указаны");

        _fullPathFileArchive = Path.Combine(_menu.Path, _nameFileArchive);
        if (File.Exists(_fullPathFileArchive))
        {
            throw new ArgumentException($"Архив с названием: {_nameFileArchive} уже сеществует в данной директории");
        }

        using var archive = ZipFile.Open(_fullPathFileArchive, ZipArchiveMode.Create);

        var files = new List<FileInfo>();
        foreach (var fileToArchive in filesToArchive)
        {
            var fullPathFileToArchive = Path.Combine(_menu.Path, fileToArchive);

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
    }

    public async Task SaveToDatabaseAsync()
    {
        if (_nameFileArchive is null || _fullPathFileArchive is null)
        {
            throw new Exception();    
        }

        var dateTimeCreateArchive = DateTime.UtcNow;

        var infoFile = new InfoFile
        {
            Filename = _nameFileArchive,
            Location = _fullPathFileArchive,
            CreatedAt = dateTimeCreateArchive,
            Size = 0,
            UserId = _menu.UserId
        };

        var operationFile = new OperationFile
        {
            OperationType = OperationTypeFile.Create,
            ExecutedAt = dateTimeCreateArchive,
            FileId = infoFile.Id,
            UserId = infoFile.UserId
        };

        await _fileRepository.AddAsync(infoFile, operationFile);
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