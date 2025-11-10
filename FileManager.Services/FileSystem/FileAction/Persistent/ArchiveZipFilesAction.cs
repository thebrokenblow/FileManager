using FileManager.Domain.Interfaces.UseCases;
using FileManager.Domain.Model;
using FileManager.Services.Exceptions;
using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;
using System.IO.Compression;

namespace FileManager.Services.FileSystem.FileAction.Persistent;

public class ArchiveZipFilesAction(
    IMenu menu,
    IFileUseCase fileUseCase) : IFileSystemCommand, IFileSystemPersistent
{
    private readonly IMenu _menu = menu ?? 
        throw new ArgumentNullException(nameof(menu));

    private readonly IFileUseCase _fileUseCase = fileUseCase ?? 
        throw new ArgumentNullException(nameof(fileUseCase));

    private long? _archiveSize;
    private string? _nameFileArchive;
    private string? _fullPathFileArchive;

    private readonly CommandValidator commandValidator = new();

    public void Execute(string command)
    {
        ResetFiled();

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

        try
        {
            using var archive = ZipFile.Open(_fullPathFileArchive, ZipArchiveMode.Create);

            var files = new List<FileInfo>();
            foreach (var fileToArchive in filesToArchive)
            {
                var fullPathFileToArchive = Path.Combine(_menu.Path, fileToArchive);

                commandValidator.ValidateCustom(
                                    () => ValidateNotExists(fullPathFileToArchive),
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

            var fileInfo = new FileInfo(_fullPathFileArchive);

            _archiveSize = 0;
            if (fileInfo.Exists)
            {
                _archiveSize = fileInfo.Length;
            }
        }
        catch (Exception)
        {
            throw new ArgumentException("Ошибка аргументов при создании архива");
        }
    }

    public async Task SaveToDatabaseAsync()
    {
        if (_archiveSize is null ||
            _nameFileArchive is null || 
            _fullPathFileArchive is null)
        {
            throw new InvalidOperationException("Некорректное поведение системы");
        }

        try
        {
            var createAt = DateTime.UtcNow;
            var fileModel = new FileModel(_nameFileArchive, _fullPathFileArchive, createAt, _archiveSize.Value);

            await _fileUseCase.CreateAsync(fileModel, _menu.UserId);
        }
        catch (Exception ex)
        {
            throw new DatabaseOperationException($"Ошибка базы данных при создании архива '{_nameFileArchive}'", ex);
        }
    }

    private void EnsureZipExtension()
    {
        if (_fullPathFileArchive is null)
        {
            throw new InvalidOperationException("Некорректное поведение системы");
        }

        if (!_fullPathFileArchive.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            _fullPathFileArchive += ".zip";
        }
    }

    private void ResetFiled()
    {
        _archiveSize = null;
        _nameFileArchive = null;
        _fullPathFileArchive = null;
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


    private static bool ValidateNotExists(string fullPathFileToArchive)
    {
        return !File.Exists(fullPathFileToArchive) && !Directory.Exists(fullPathFileToArchive);
    }
}