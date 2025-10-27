using FileManager.Data.Repositories;
using FileManager.Extensions;
using FileManager.Utils;

namespace FileManager.Actions.DirectoryAction;

public class MoveDirectoriesAction(
    Menu menu, 
    DirectoryPath directoryPath, 
    DirectoryRepository directoryRepository) : IAction
{
    private const int CountArguments = 3;
    private const string ArgumentMoveDirectoryBelow = "-l";

    public void Execute(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            throw new ArgumentException("Команда не может быть пустой");
        }

        var arguments = command.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
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

        var fullPathSourceDirectory = Path.Combine(menu.CurrentPath, nameSourceDirectory);
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

        var fullPathDestinationDirectory = Path.Combine(menu.CurrentPath, nameDestinationDirectory);
        if (!Directory.Exists(fullPathDestinationDirectory))
        {
            throw new ArgumentException($"Не существует директории с названием: {nameDestinationDirectory}");
        }

        fullPathDestinationDirectory = Path.Combine(fullPathDestinationDirectory, nameSourceDirectory);
        if (Directory.Exists(fullPathDestinationDirectory))
        {
            throw new ArgumentException($"В директории {nameSourceDirectory} уже существует директория {nameDestinationDirectory}");
        }

        MoveDirectory(fullPathSourceDirectory, fullPathDestinationDirectory);

        var infoDirectory = directoryRepository.GetByLocation(fullPathSourceDirectory);
        infoDirectory.Location = fullPathDestinationDirectory;
        directoryRepository.Update(infoDirectory);
    }

    private void MoveDirectoryToDirectoryBelow(string nameSourceDirectory, string fullPathSourceDirectory)
    {
        var directoryBelow = directoryPath.GetDirectoryBelow();
        string fullPathDirectoryBelow = Path.Combine(directoryBelow, nameSourceDirectory);

        if (Directory.Exists(fullPathDirectoryBelow))
        {
            throw new ArgumentException($"Уже существует директория: {nameSourceDirectory} на уровне ниже");
        }

        MoveDirectory(fullPathSourceDirectory, fullPathDirectoryBelow);

        var infoDirectory = directoryRepository.GetByLocation(fullPathSourceDirectory);
        infoDirectory.Location = fullPathDirectoryBelow;
        directoryRepository.Update(infoDirectory);
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