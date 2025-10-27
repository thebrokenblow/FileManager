using FileManager.Extensions;
using FileManager.Utils;

namespace FileManager.Actions.FileAction;

public class ReadFileAction(Menu menu) : IAction
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
        if (!nameCommand.Equals(CommandDictionary.ReadFile, StringComparison.CurrentCultureIgnoreCase))
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
            throw new ArgumentException($"Файла с именем: {nameFile} несуществует");
        }

        try
        {
            var text = File.ReadAllText(fullPath);
            Console.WriteLine(text);
        }
        catch
        {
            throw new ArgumentException($"Ошибка чтения файла");
        }
    }
}