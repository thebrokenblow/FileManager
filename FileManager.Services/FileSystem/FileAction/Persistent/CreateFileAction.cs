using FileManager.Domain.Entities;
using FileManager.Domain.Entities.Enums;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;

namespace FileManager.Services.FileSystem.FileAction.Persistent;

public class CreateFileAction(
    IMenu menu, 
    IFileRepository fileRepository) : IFileSystemAction, IFileSystemPersistentAction
{
    private const int CountArguments = 2;

    private readonly IMenu _menu = menu ??
        throw new ArgumentNullException(nameof(menu));

    private readonly IFileRepository _fileRepository = fileRepository ??
        throw new ArgumentNullException(nameof(fileRepository));

    private readonly CommandValidator commandValidator = new();

    private string? _nameFile;
    private string? _fullPath;

    public void Execute(string command)
    {
        commandValidator
            .ValidateNotEmpty(command, "Команда не может быть пустой")
            .ValidateArgumentsCount(out string[] arguments, command, CountArguments, $"Некорректное количество аргументов: {command}");

        var nameCommand = arguments.First();
        _nameFile = arguments.Second();
        _fullPath = Path.Combine(_menu.Path, _nameFile);

        commandValidator
            .ValidateCommandName(nameCommand, CommandDictionary.CreateFile, $"Команда: {command} не распознана")
            .ValidateNotEmpty(_nameFile, "Имя файла не может быть пустым")
            .ValidatePathSecurity(_nameFile, $"Недопустимое имя файла: {_nameFile}")
            .ValidateFileNotExists(_fullPath, $"Уже существует такой файл: {_nameFile}");

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
            throw new InvalidOperationException("Некорректное поведение системы");
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