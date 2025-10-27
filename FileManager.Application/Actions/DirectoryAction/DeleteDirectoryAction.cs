using FileManager.Application.Actions.Interfaces;
using FileManager.Application.Extensions;
using FileManager.Application.Utils;

namespace FileManager.Application.Actions.DirectoryAction;

public class DeleteDirectoryAction(
    IMenu menu, 
    DirectoryRepository directoryRepository, 
    OperationDirectoryRepository operationDirectoryRepository) : IAction
{
    private const int CountArguments = 2;

    public  async Task Execute(string command)
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

        var fullPathDirectory = Path.Combine(menu.Path, nameDirectory);
        if (!Directory.Exists(fullPathDirectory))
        {
            throw new ArgumentException($"Нет дериктории с именем: {nameDirectory}");
        }

        try
        {
            Directory.Delete(fullPathDirectory, true);
            await AddOperationToDatabaseAsync(fullPathDirectory);
        }
        catch
        {
            throw new ArgumentException($"Ошибка удаления файла");
        }
    }

    private async Task AddOperationToDatabaseAsync(string fullPathDirectory)
    {
        var directoryId = directoryRepository.GetIdByLocation(fullPathDirectory);

        var operationDirectory = new OperationDirectory
        {
            DirectoryId = directoryId,
            Timestamp = DateTime.UtcNow,
            OperationType = OperationTypeDirectory.Delete,
            UserId = menu.UserId
        };

        await operationDirectoryRepository.AddAsync(operationDirectory);
    }
}