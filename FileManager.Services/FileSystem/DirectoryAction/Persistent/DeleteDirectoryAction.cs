using FileManager.Domain.Entities;
using FileManager.Domain.Entities.Enums;
using FileManager.Domain.Interfaces.Queries;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;

namespace FileManager.Services.FileSystem.DirectoryAction.Persistent;

public class DeleteDirectoryAction(
    IMenu menu, 
    IDirectoryQueries directoryQueries, 
    IOperationDirectoryRepository operationDirectoryRepository) : IFileSystemAction, IFileSystemPersistentAction
{
    private const int CountArguments = 2;

    private readonly IMenu _menu = menu ??
        throw new ArgumentNullException(nameof(menu));

    private readonly IDirectoryQueries _directoryQueries = directoryQueries ??
        throw new ArgumentNullException(nameof(directoryQueries));

    private readonly IOperationDirectoryRepository _operationDirectoryRepository = operationDirectoryRepository ??
        throw new ArgumentNullException(nameof(operationDirectoryRepository));

    private readonly CommandValidator commandValidator = new();

    private string? _fullPathDirectory;

    public void Execute(string command)
    {
        commandValidator
            .ValidateNotEmpty(command, "Команда не может быть пустой")
            .ValidateArgumentsCount(out string[] arguments, command, CountArguments, $"Некорректное количество аргументов: {command}");

        var nameCommand = arguments.First();
        var nameDirectory = arguments.Second();
        _fullPathDirectory = Path.Combine(_menu.Path, nameDirectory);

        commandValidator
            .ValidateCommandName(nameCommand, CommandDictionary.DeleteDirectory, $"Команда: {command} не распознана")
            .ValidateNotEmpty(nameDirectory, "Имя директории не может быть пустым")
            .ValidatePathSecurity(nameDirectory, $"Недопустимое имя директории: {nameDirectory}")
            .ValidateDirectoryExists(_fullPathDirectory, $"Нет дериктории с именем: {nameDirectory}");

        try
        {
            Directory.Delete(_fullPathDirectory, true);
        }
        catch
        {
            throw new ArgumentException($"Ошибка удаления файла");
        }
    }

    public async Task SaveToDatabaseAsync()
    {
        if (_fullPathDirectory is null)
        {
            throw new InvalidOperationException("Некорректное поведение системы");
        }

        var directoryId = await _directoryQueries.GetIdByLocationAsync(_fullPathDirectory) ??
                                    throw new InvalidOperationException("Некорректное поведение системы, проблема с базой данных");

        var operationDirectory = new OperationDirectory
        {
            OperationType = OperationTypeDirectory.Delete,
            ExecutedAt = DateTime.UtcNow,
            DirectoryId = directoryId,
            UserId = _menu.UserId
        };

        await _operationDirectoryRepository.AddAsync(operationDirectory);
    }
}