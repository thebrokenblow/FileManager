using FileManager.Application.Actions.Interfaces;
using FileManager.Application.Extensions;
using FileManager.Application.Utils;

namespace FileManager.Application.Actions.DirectoryAction;

public class CreateDirectoryAction(IMenu menu, DirectoryRepository directoryRepository) : IAction
{
    private const int CountArguments = 2;

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
        if (!nameCommand.Equals(CommandDictionary.CreateDirectory, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException($"Команда: {command} не распознана");
        }

        var nameDirectory = arguments.Second();
        if (string.IsNullOrWhiteSpace(nameDirectory))
        {
            throw new ArgumentException("Имя директории не может быть пустым");
        }

        if (PathSecurity.IsPathTraversal(nameDirectory))
        {
            throw new ArgumentException($"Недопустимое имя директории: {nameDirectory}");
        }

        var fullPathDirectory = Path.Combine(menu.Path, nameDirectory);
        if (Directory.Exists(fullPathDirectory))
        {
            throw new ArgumentException($"Уже существует такая директория: {nameDirectory}");
        }

        try
        {
            Directory.CreateDirectory(fullPathDirectory);
            directoryRepository.Create(menu.UserId, nameDirectory, fullPathDirectory);
        }
        catch
        {
            throw new ArgumentException($"Ошибка создания директории");
        }
    }
}