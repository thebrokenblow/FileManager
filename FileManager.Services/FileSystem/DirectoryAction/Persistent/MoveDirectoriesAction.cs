using FileManager.Domain.Interfaces.Queries;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;

namespace FileManager.Services.FileSystem.DirectoryAction.Persistent;

public class MoveDirectoriesAction(
    IMenu menu,
    IDirectoryQueries directoryQueries,
    IDirectoryRepository directoryRepository,
    DirectoryPath directoryPath) : IFileSystemAction, IFileSystemPersistentAction
{
    private const int CountArguments = 3;
    private const string ArgumentMoveDirectoryBelow = "-l";

    private readonly IMenu _menu = menu ??
        throw new ArgumentNullException(nameof(menu));

    private readonly IDirectoryQueries _directoryQueries = directoryQueries ??
        throw new ArgumentNullException(nameof(directoryQueries));

    private readonly IDirectoryRepository _directoryRepository = directoryRepository ??
        throw new ArgumentNullException(nameof(directoryRepository));

    private readonly CommandValidator commandValidator = new();

    private string? _newFullPathDestinationDirectory;

    public void Execute(string command)
    {
        commandValidator
            .ValidateNotEmpty(command, "Команда не может быть пустой")
            .ValidateArgumentsCount(out string[] arguments, command, CountArguments, $"Некорректное количество аргументов: {command}");

        var nameCommand = arguments.First();
        var nameSourceDirectory = arguments.Second();
        var fullPathSourceDirectory = Path.Combine(_menu.Path, nameSourceDirectory);

        commandValidator
            .ValidateCommandName(nameCommand, CommandDictionary.MoveDirectory, $"Команда не распознана: {command}")
            .ValidateNotEmpty(nameSourceDirectory, "Название директории не может быть пустым")
            .ValidateDirectoryExists(fullPathSourceDirectory, $"Не существует директории с названием: {nameSourceDirectory}")
            .ValidatePathSecurity(nameSourceDirectory, $"Недопустимое имя директории: {nameSourceDirectory}");

        var nameDestinationDirectory = arguments.Third();
        if (nameDestinationDirectory.Equals(ArgumentMoveDirectoryBelow, StringComparison.CurrentCultureIgnoreCase))
        {
            MoveDirectoryToDirectoryBelow(nameSourceDirectory, fullPathSourceDirectory);
            return;
        }

        var fullPathDestinationDirectory = Path.Combine(_menu.Path, nameDestinationDirectory);
        _newFullPathDestinationDirectory = Path.Combine(fullPathDestinationDirectory, nameSourceDirectory);

        commandValidator
            .ValidateNotEmpty(nameDestinationDirectory, "Название директории не может быть пустым")
            .ValidatePathSecurity(nameDestinationDirectory, $"Недопустимое имя директории: {nameDestinationDirectory}")
            .ValidateDirectoryExists(fullPathDestinationDirectory, $"Не существует директории с названием: {nameDestinationDirectory}")
            .ValidateDirectoryExists(_newFullPathDestinationDirectory, $"В директории {nameSourceDirectory} уже существует директория {nameDestinationDirectory}");
        
        MoveDirectory(fullPathSourceDirectory, _newFullPathDestinationDirectory);
    }

    public async Task SaveToDatabaseAsync()
    {
        if (_newFullPathDestinationDirectory is null)
        {
            throw new InvalidOperationException("Некорректное поведение системы");
        }

        var infoDirectory = await _directoryQueries.GetByLocationAsync(_newFullPathDestinationDirectory) ??
                                        throw new Exception("Отсутствует соответствующая запись директории");

        infoDirectory.Location = _newFullPathDestinationDirectory;
        await _directoryRepository.UpdateAsync(infoDirectory);
    }

    private void MoveDirectoryToDirectoryBelow(string nameSourceDirectory, string fullPathSourceDirectory)
    {
        var directoryBelow = directoryPath.GetDirectoryBelow();
        _newFullPathDestinationDirectory = Path.Combine(directoryBelow, nameSourceDirectory);

        if (Directory.Exists(_newFullPathDestinationDirectory))
        {
            throw new ArgumentException($"Уже существует директория: {nameSourceDirectory} на уровне ниже");
        }

        MoveDirectory(fullPathSourceDirectory, _newFullPathDestinationDirectory);   
    }

    private static void MoveDirectory(string sourceDirName, string destDirName)
    {
        try
        {
            Directory.Move(sourceDirName, destDirName);
        }
        catch
        {
            throw new ArgumentException($"Ошибка перемещения деректории");
        }
    }
}