using FileManager.Domain.Entities;
using FileManager.Domain.Entities.Enums;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;
using System.IO.Compression;

namespace FileManager.Services.FileSystem.FileAction.Persistent;

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

    private readonly CommandValidator commandValidator = new();

    public void Execute(string command)
    {
        commandValidator
           .ValidateNotEmpty(command, "Команда не может быть пустой");

        var arguments = command.Split(WhitespaceCharsDictionary.AllWhitespace, StringSplitOptions.RemoveEmptyEntries);

        var nameCommand = arguments.First();
        _nameFileArchive = arguments.Second();
        _fullPathFileArchive = Path.Combine(_menu.Path, _nameFileArchive);

        commandValidator
            .ValidateCommandName(nameCommand, CommandDictionary.ArchiveZip, "Команда не может быть пустой")
            .ValidateNotEmpty(_nameFileArchive, "Название файла архива не может быть пустым")
            .ValidatePathSecurity(_nameFileArchive, $"Недопустимое имя файла архива: {_nameFileArchive}")
            .ValidateFileNotExists(_fullPathFileArchive, $"Архив с названием: {_nameFileArchive} уже сеществует в данной директории");

        EnsureZipExtension();

        var filesToArchive = arguments[2..] ??
                                throw new ArgumentException("Файлы для архива не указаны");

        using var archive = ZipFile.Open(_fullPathFileArchive, ZipArchiveMode.Create);

        var files = new List<FileInfo>();
        foreach (var fileToArchive in filesToArchive)
        {
            var fullPathFileToArchive = Path.Combine(_menu.Path, fileToArchive);

            commandValidator.ValidateCustom(
                                () => !File.Exists(fullPathFileToArchive) && !Directory.Exists(fullPathFileToArchive),
                                $"Не существует файла или директории: {fileToArchive} в текущей директории")
                            .ValidatePathSecurity(fileToArchive, $"Недопустимое имя или дериктории: {fileToArchive}");

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
            throw new InvalidOperationException("Некорректное поведение системы");
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

    private void EnsureZipExtension()
    {
        if (_nameFileArchive is null)
        {
            throw new InvalidOperationException("Некорректное поведение системы");
        }

        if (!_nameFileArchive.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            _nameFileArchive += ".zip";
        }
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