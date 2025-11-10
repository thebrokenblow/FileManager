using FileManager.Domain.Interfaces.UseCases;
using FileManager.Domain.Model;
using FileManager.Services.Exceptions;
using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;

namespace FileManager.Services.FileSystem.DirectoryAction.Persistent;

public class MoveDirectoriesAction(
    IMenu menu,
    IDirectoryUseCase directoryUseCase,
    DirectoryPath directoryPath) : IFileSystemCommand, IFileSystemPersistent
{
    private const int CountArguments = 3;

    public const string ArgumentMoveDirectoryBelow = "-l";

    private readonly IMenu _menu = menu ??
        throw new ArgumentNullException(nameof(menu));

    private readonly IDirectoryUseCase _directoryUseCase = directoryUseCase ??
        throw new ArgumentNullException(nameof(directoryUseCase));

    private readonly CommandValidator commandValidator = new();

    private string? _fullPathSourceDirectory;
    private string? _fullPathDestinationDirectory;
    private string? _newFullPathDestinationDirectory;

    public void Execute(string command)
    {
        ResetFiled();

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

        _fullPathSourceDirectory = Path.Combine(_menu.Path, nameSourceDirectory);
        _fullPathDestinationDirectory = Path.Combine(_menu.Path, nameDestinationDirectory);
        _newFullPathDestinationDirectory = Path.Combine(_fullPathDestinationDirectory, nameSourceDirectory);

        commandValidator
            .ValidateNotEmpty(nameDestinationDirectory, "Название директории не может быть пустым")
            .ValidatePathSecurity(nameDestinationDirectory, $"Недопустимое имя директории: {nameDestinationDirectory}")
            .ValidateDirectoryExists(_fullPathDestinationDirectory, $"Не существует директории с названием: {nameDestinationDirectory}")
            .ValidateDirectoryNotExists(_newFullPathDestinationDirectory, $"В директории {nameDestinationDirectory} уже существует директория {nameSourceDirectory}");
        
        MoveDirectory(fullPathSourceDirectory, _newFullPathDestinationDirectory);
    }

    public async Task SaveToDatabaseAsync()
    {
        if (_fullPathSourceDirectory is null ||
            _fullPathDestinationDirectory is null ||
            _newFullPathDestinationDirectory is null)
        {
            throw new InvalidOperationException("Некорректное поведение системы");
        }

        try
        {
            var dateTimeMoveDirectory = DateTime.UtcNow;

            var moveDirectoryModel = new MoveDirectoryModel(
                dateTimeMoveDirectory,
                _fullPathSourceDirectory,
                _fullPathDestinationDirectory,
                _newFullPathDestinationDirectory);

            await _directoryUseCase.MoveAsync(moveDirectoryModel, _menu.UserId);
        }
        catch (Exception ex)
        {
            throw new DatabaseOperationException($"Ошибка базы данных при перемещении директории '{_fullPathSourceDirectory}'", ex);
        }
    }

    private void MoveDirectoryToDirectoryBelow(string nameSourceDirectory, string fullPathSourceDirectory)
    {
        var directoryBelow = directoryPath.GetDirectoryBelow();

        _fullPathSourceDirectory = fullPathSourceDirectory;
        _newFullPathDestinationDirectory = Path.Combine(directoryBelow, nameSourceDirectory);

        commandValidator
            .ValidateDirectoryNotExists(_newFullPathDestinationDirectory, $"Уже существует директория: {nameSourceDirectory} на уровне ниже");

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

    private void ResetFiled()
    {
        _fullPathSourceDirectory = null;
        _fullPathDestinationDirectory = null;
        _newFullPathDestinationDirectory = null;
    }
}