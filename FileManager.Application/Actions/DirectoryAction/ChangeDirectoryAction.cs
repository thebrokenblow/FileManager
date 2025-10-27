using FileManager.Application.Actions.Interfaces;
using FileManager.Application.Extensions;
using FileManager.Application.Utils;

namespace FileManager.Application.Actions.DirectoryAction;

public class ChangeDirectoryAction(IMenu menu, DirectoryPath directoryPath) : IAction
{
    private const string SymbolMoveToDirectoryBelow = "..";

    private const string ExclusiveCaseMoveToDirectoryBelow = 
            $"{CommandDictionary.ChangeDirectory}{SymbolMoveToDirectoryBelow}";

    private const int CountArguments = 2;

    public void Execute(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            throw new ArgumentException("Команда не может быть пустой");
        }

        var arguments = command.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

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

        var fullPath = Path.Combine(menu.Path, nameDirectory);
        MoveDirectory(fullPath);
    }

    private void MoveDirectory(string fullPath)
    {
        if (!Directory.Exists(fullPath))
        {
            throw new ArgumentException($"Нет дериктории с именем: {fullPath}");
        }

        menu.ChangePath(fullPath);
    }

    private void MoveDirectoryBelow()
    {
        var directoryBelow = directoryPath.GetDirectoryBelow();
        menu.ChangePath(directoryBelow);
    }
}