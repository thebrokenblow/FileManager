using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;

namespace FileManager.Services.FileSystem.FileAction.Transient;

public class ReadFileAction(IMenu menu) : IFileSystemCommand
{
    private const int CountArguments = 2;

    private readonly IMenu _menu = menu ??
        throw new ArgumentNullException(nameof(menu));

    private readonly CommandValidator commandValidator = new();

    public void Execute(string command)
    {
        commandValidator
          .ValidateNotEmpty(command, "Команда не может быть пустой")
          .ValidateArgumentsCount(out string[] arguments, command, CountArguments, $"Некорректное количество аргументов: {command}");

        var nameCommand = arguments.First();
        var nameFile = arguments.Second();
        var fullPath = Path.Combine(_menu.Path, nameFile);

        commandValidator
            .ValidateCommandName(nameCommand, CommandDictionary.ReadFile, $"Команда: {command} не распознана")
            .ValidateNotEmpty(nameFile, "Имя файла не может быть пустым")
            .ValidatePathSecurity(nameFile, $"Недопустимое имя файла: {nameFile}")
            .ValidateFileExists(fullPath, $"Файла с именем: {nameFile} несуществует");

        try
        {
            var text = File.ReadAllText(fullPath);
            menu.Output.Invoke(text);
        }
        catch
        {
            throw new ArgumentException($"Ошибка чтения файла");
        }
    }
}