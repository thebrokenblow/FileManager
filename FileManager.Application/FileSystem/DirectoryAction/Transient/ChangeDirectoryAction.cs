using FileManager.Application.Extensions;
using FileManager.Application.FileSystem.Interfaces;
using FileManager.Application.Utils;

namespace FileManager.Application.FileSystem.DirectoryAction.Transient;

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

    public void Execute(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            throw new ArgumentException("Команда не может быть пустой");
        }

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
        if (!nameCommand.Equals(CommandDictionary.ChangeDirectory, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException($"Команда не распознана: {command}");
        }

        var nameDirectory = arguments.Second();
        if (string.IsNullOrWhiteSpace(nameDirectory))
        {
            throw new ArgumentException("Имя директории не может быть пустым");
        }

        if (nameDirectory == SymbolMoveToDirectoryBelow)
        {
            MoveDirectoryBelow();
            return;
        }

        if (PathSecurity.IsPathTraversal(nameDirectory))
        {
            throw new ArgumentException($"Некорректные аргументы: {nameDirectory}");
        }

        var fullPath = Path.Combine(_menu.Path, nameDirectory);
        MoveDirectory(fullPath);
    }

    private void MoveDirectory(string fullPath)
    {
        if (!Directory.Exists(fullPath))
        {
            throw new ArgumentException($"Нет дериктории с именем: {fullPath}");
        }

        _menu.ChangePath(fullPath);
    }

    private void MoveDirectoryBelow()
    {
        var directoryBelow = directoryPath.GetDirectoryBelow();
        _menu.ChangePath(directoryBelow);
    }
}