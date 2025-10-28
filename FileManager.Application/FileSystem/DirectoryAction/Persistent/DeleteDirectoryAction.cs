using FileManager.Application.Extensions;
using FileManager.Application.FileSystem.Interfaces;
using FileManager.Application.Utils;
using FileManager.Domain.Entities;
using FileManager.Domain.Entities.Enums;
using FileManager.Domain.Interfaces.Queries;
using FileManager.Domain.Interfaces.Repositories;

namespace FileManager.Application.FileSystem.DirectoryAction.Persistent;


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

    private string? _fullPathDirectory;

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
        if (!nameCommand.Equals(CommandDictionary.DeleteDirectory, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException($"Команда не распознана: {command}");
        }

        var nameDirectory = arguments.Second();
        if (string.IsNullOrWhiteSpace(nameDirectory))
        {
            throw new ArgumentException("Название директории не может быть пустой");
        }

        if (PathSecurity.IsPathTraversal(nameDirectory))
        {
            throw new ArgumentException($"Недопустимое имя директории: {nameDirectory}");
        }

        _fullPathDirectory = Path.Combine(menu.Path, nameDirectory);
        if (!Directory.Exists(_fullPathDirectory))
        {
            throw new ArgumentException($"Нет дериктории с именем: {nameDirectory}");
        }

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
            throw new Exception();    
        }

        var directoryId = await _directoryQueries.GetIdByLocationAsync(_fullPathDirectory) ??
                                    throw new Exception("Отсутствует соответствующая запись директории");

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