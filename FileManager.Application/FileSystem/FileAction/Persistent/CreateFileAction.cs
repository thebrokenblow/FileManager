using FileManager.Application.Extensions;
using FileManager.Application.FileSystem.Interfaces;
using FileManager.Application.Utils;
using FileManager.Domain.Entities;
using FileManager.Domain.Entities.Enums;
using FileManager.Domain.Interfaces.Repositories;

namespace FileManager.Application.FileSystem.FileAction.Persistent;

public class CreateFileAction(
    IMenu menu, 
    IFileRepository fileRepository) : IFileSystemAction, IFileSystemPersistentAction
{
    private const int CountArguments = 2;

    private readonly IMenu _menu = menu ??
        throw new ArgumentNullException(nameof(menu));

    private readonly IFileRepository _fileRepository = fileRepository ??
        throw new ArgumentNullException(nameof(fileRepository));

    private string? _nameFile;
    private string? _fullPath;

    public void Execute(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            throw new ArgumentException("Команда не может быть пустой");
        }

        var arguments = command.Split(WhitespaceCharsDictionary.AllWhitespace, StringSplitOptions.RemoveEmptyEntries);
        if (arguments.Length != CountArguments)
        {
            throw new ArgumentException($"Некорректное количество аргументов: {command}");
        }

        var nameCommand = arguments.First();
        if (!nameCommand.Equals(CommandDictionary.CreateFile, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException($"Команда: {command} не распознана");
        }

        _nameFile = arguments.Second();
        if (string.IsNullOrWhiteSpace(_nameFile))
        {
            throw new ArgumentException("Имя файла не может быть пустым");
        }

        if (PathSecurity.IsPathTraversal(_nameFile))
        {
            throw new ArgumentException($"Недопустимое имя файла: {_nameFile}");
        }

        _fullPath = Path.Combine(_menu.Path, _nameFile);
        if (File.Exists(_fullPath))
        {
            throw new ArgumentException($"Уже существует такой файл: {_nameFile}");
        }

        try
        {
            var file = File.Create(_fullPath);
            file.Dispose();
        }
        catch
        {
            throw new ArgumentException($"Ошибка создания файла");
        }   
    }

    public async Task SaveToDatabaseAsync()
    {
        if (_nameFile is null || _fullPath is null)
        {
            throw new Exception();
        }

        var dateTimeCreateArchive = DateTime.UtcNow;

        var infoFile = new InfoFile
        {
            Filename = _nameFile,
            Location = _fullPath,
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
}