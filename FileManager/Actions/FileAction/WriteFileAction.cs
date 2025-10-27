using FileManager.Data.Entities;
using FileManager.Data.Repositories;
using FileManager.Extensions;
using FileManager.Utils;

namespace FileManager.Actions.FileAction;

public class WriteFileAction(
    Menu menu, 
    FileRepository fileRepository, 
    InfoFileRepository infoFileRepository) : IAction
{
    private const int CountArguments = 2;
    private string? fullPathFile;

    public void Execute(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            throw new ArgumentException("Команда не может быть пустой");
        }

        var arguments = command.Split([' '], StringSplitOptions.RemoveEmptyEntries);
        if (arguments.Length != CountArguments)
        {
            throw new ArgumentException($"Некорректное количество аргументов: {command}");
        }

        var nameCommand = arguments.First();
        if (!nameCommand.Equals(CommandDictionary.WriteFile, StringComparison.CurrentCultureIgnoreCase))
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

        fullPathFile = Path.Combine(menu.CurrentPath, nameFile);
        if (!File.Exists(fullPathFile))
        {
            throw new ArgumentException($"Файла с именем: {nameFile} несуществует");
        }

        var text = File.ReadAllText(fullPathFile);
        InputPanel.Input(WriteFile, text);

        var id = fileRepository.GetIdByLocation(fullPathFile);
        var infoFile = new InfoFile
        {
            Filename = nameFile,
            CreatedAt = DateTime.UtcNow,
            Location = fullPathFile,
            UserId = menu.UserId,
        };
        infoFileRepository.Add(infoFile);
    }

    private void WriteFile(string? contents)
    {
        if (fullPathFile is null)
        {
            throw new ArgumentException("Не установлено название файла");
        }

        try
        {
            File.WriteAllText(fullPathFile, contents);
        }
        catch
        {
            throw new ArgumentException("Ошибка записи в файла");
        }
    }
}