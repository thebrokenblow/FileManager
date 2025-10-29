using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;

namespace FileManager.Services.FileSystem.DirectoryAction.Transient;

public class ChangeDirectoryAction(
    IMenu menu, 
    DirectoryPath directoryPath) : IFileSystemAction
{
    private const string SymbolMoveToDirectoryBelow = "..";

    private const string ExclusiveCaseMoveToDirectoryBelow = 
            $"{CommandDictionary.ChangeDirectory}{SymbolMoveToDirectoryBelow}";

    private const int CountArguments = 2;

    private readonly IMenu _menu = menu ??
        throw new ArgumentNullException(nameof(menu));

    private readonly CommandValidator commandValidator = new();

    public void Execute(string command)
    {
        commandValidator
            .ValidateNotEmpty(command, "Команда не может быть пустой")
            .ValidateArgumentsCount(out string[] arguments, command, CountArguments, $"Некорректное количество аргументов: {command}");

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
            .ValidateDirectoryExists(nameDirectory, $"Нет дериктории с именем: {fullPath}");

        _menu.ChangePath(fullPath);
    }

    private void MoveDirectoryBelow()
    {
        var directoryBelow = directoryPath.GetDirectoryBelow();
        _menu.ChangePath(directoryBelow);
    }
}