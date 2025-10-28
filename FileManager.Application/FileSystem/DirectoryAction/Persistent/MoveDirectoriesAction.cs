using FileManager.Application.Extensions;
using FileManager.Application.FileSystem.Interfaces;
using FileManager.Application.Utils;
using FileManager.Domain.Interfaces.Queries;
using FileManager.Domain.Interfaces.Repositories;

namespace FileManager.Application.FileSystem.DirectoryAction.Persistent;

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

    private string? _fullPathDestinationDirectory;

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
        if (!nameCommand.Equals(CommandDictionary.MoveDirectory, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException($"Команда не распознана: {command}");
        }

        var nameSourceDirectory = arguments.Second();
        if (string.IsNullOrWhiteSpace(nameSourceDirectory))
        {
            throw new ArgumentException("Название директории не может быть пустым");
        }

        var fullPathSourceDirectory = Path.Combine(_menu.Path, nameSourceDirectory);
        if (!Directory.Exists(fullPathSourceDirectory))
        {
            throw new ArgumentException($"Не существует директории с названием: {nameSourceDirectory}");
        }

        if (PathSecurity.IsPathTraversal(nameSourceDirectory))
        {
            throw new ArgumentException($"Недопустимое имя директории: {nameSourceDirectory}");
        }

        var nameDestinationDirectory = arguments.Third();
        if (nameDestinationDirectory.Equals(ArgumentMoveDirectoryBelow, StringComparison.CurrentCultureIgnoreCase))
        {
            MoveDirectoryToDirectoryBelow(nameSourceDirectory, fullPathSourceDirectory);
            return;
        }

        if (string.IsNullOrWhiteSpace(nameDestinationDirectory))
        {
            throw new ArgumentException("Название директории не может быть пустым");
        }

        if (PathSecurity.IsPathTraversal(nameDestinationDirectory))
        {
            throw new ArgumentException($"Недопустимое имя директории: {nameDestinationDirectory}");
        }

        var fullPathDestinationDirectory = Path.Combine(_menu.Path, nameDestinationDirectory);
        if (!Directory.Exists(fullPathDestinationDirectory))
        {
            throw new ArgumentException($"Не существует директории с названием: {nameDestinationDirectory}");
        }

        _fullPathDestinationDirectory = Path.Combine(fullPathDestinationDirectory, nameSourceDirectory);
        if (Directory.Exists(fullPathDestinationDirectory))
        {
            throw new ArgumentException($"В директории {nameSourceDirectory} уже существует директория {nameDestinationDirectory}");
        }
    }

    public async Task SaveToDatabaseAsync()
    {
        if (_fullPathDestinationDirectory is null)
        {
            throw new Exception();
        }

        var infoDirectory = await _directoryQueries.GetByLocationAsync(_fullPathDestinationDirectory) ??
                                        throw new Exception("Отсутствует соответствующая запись директории");

        infoDirectory.Location = _fullPathDestinationDirectory;
        await _directoryRepository.UpdateAsync(infoDirectory);
    }

    private void MoveDirectoryToDirectoryBelow(string nameSourceDirectory, string fullPathSourceDirectory)
    {
        var directoryBelow = directoryPath.GetDirectoryBelow();
        _fullPathDestinationDirectory = Path.Combine(directoryBelow, nameSourceDirectory);

        if (Directory.Exists(_fullPathDestinationDirectory))
        {
            throw new ArgumentException($"Уже существует директория: {nameSourceDirectory} на уровне ниже");
        }

        MoveDirectory(fullPathSourceDirectory, _fullPathDestinationDirectory);   
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