using FileManager.Domain.Interfaces.UseCases;
using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;

namespace FileManager.Services.FileSystem.DirectoryAction.Transient;

public class ChangeDirectoryAction(
    IMenu menu, 
    DirectoryPath directoryPath) : IFileSystemCommand
{
    private const string SymbolMoveToDirectoryBelow = "..";

    private const string ExclusiveCaseMoveToDirectoryBelow = 
            $"{CommandDictionary.ChangeDirectory}{SymbolMoveToDirectoryBelow}";

    private const int CountArguments = 2;

    private readonly IMenu _menu = menu ??
        throw new ArgumentNullException(nameof(menu));

    private readonly DirectoryPath _directoryPath = directoryPath ??
        throw new ArgumentNullException(nameof(directoryPath));

    private readonly CommandValidator commandValidator = new();

    public void Execute(string command)
    {
        commandValidator
            .ValidateNotEmpty(command, "Команда не может быть пустой");

        var arguments = command.Split(WhitespaceCharsDictionary.AllWhitespace, StringSplitOptions.RemoveEmptyEntries);

        if ((arguments.Length < CountArguments || arguments.Length > CountArguments) && 
            !command.Equals(ExclusiveCaseMoveToDirectoryBelow, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException($"Некорректные аргументы: {command}");
        }

        if (command == ExclusiveCaseMoveToDirectoryBelow)
        {
            MoveDirectoryBelow();
            return;
        }

        var nameCommand = arguments.First();
        var nameDirectory = arguments.Second();

        commandValidator
            .ValidateCommandName(nameCommand, CommandDictionary.ChangeDirectory, $"Команда не распознана: {command}")
            .ValidateNotEmpty(nameDirectory, "Имя директории не может быть пустым");

        if (nameDirectory == SymbolMoveToDirectoryBelow)
        {
            MoveDirectoryBelow();
            return;
        }

        var fullPath = Path.Combine(_menu.Path, nameDirectory);

        commandValidator
            .ValidatePathSecurity(nameDirectory, $"Некорректные аргументы: {nameDirectory}")
            .ValidateDirectoryExists(fullPath, $"Нет дериктории с именем: {nameDirectory}");

        _menu.Path = fullPath;
    }

    private void MoveDirectoryBelow()
    {
        var directoryBelow = _directoryPath.GetDirectoryBelow();
        _menu.Path = directoryBelow;
    }
}