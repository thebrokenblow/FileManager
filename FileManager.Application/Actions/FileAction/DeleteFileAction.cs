using FileManager.Application.Actions.Interfaces;
using FileManager.Application.Extensions;
using FileManager.Application.Utils;
using FileManager.Data.Repositories;
using FileManager.Extensions;

namespace FileManager.Application.Actions.FileAction;

public class DeleteFileAction(Menu menu, FileRepository fileRepository) : IAction
{
    private const int CountArguments = 2;

    public void Execute(string command)
    {
        if (string.IsNullOrWhiteSpace(command) )
        {
            throw new ArgumentException("Команда не может быть пустой");
        }

        var arguments = command.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        if (arguments.Length != CountArguments)
        {
            throw new ArgumentException($"Некорректное количество аргументов: {command}");
        }

        var nameCommand = arguments.First();
        if (!nameCommand.Equals(CommandDictionary.DeleteFile, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException($"Команда: {command} не распознана");
        }

        var nameFile = arguments.Second();
        if (string.IsNullOrWhiteSpace(nameFile))
        {
            throw new ArgumentException("Имя файла не может быть пустым");
        }

        if (PathSecurity.IsPathTraversal(nameFile))
        {
            throw new ArgumentException($"Недопустимое имя файла: {nameFile}");
        }

        var fullPath = Path.Combine(menu.CurrentPath, nameFile);
        if (!File.Exists(fullPath))
        {
            throw new ArgumentException($"Не существует файла: {nameFile} в текущей директории");
        }

        try
        {
            File.Delete(fullPath);

            var id = fileRepository.GetIdByLocation(fullPath);
            fileRepository.Delete(menu.UserId, id);
        }
        catch
        {
            throw new ArgumentException($"Ошибка удаления файла");
        }
    }
}