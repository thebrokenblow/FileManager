using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;
using System.Text;

namespace FileManager.Services.FileSystem.DirectoryAction.Transient;

public class ShowDirectoriesAction(IMenu menu) : IFileSystemAction
{
    private readonly IMenu _menu = menu ?? 
        throw new ArgumentNullException(nameof(menu));

    private readonly CommandValidator commandValidator = new();

    public void Execute(string command)
    {
        commandValidator
            .ValidateNotEmpty(command, "Команда не может быть пустой")
            .ValidateCommandName(command, CommandDictionary.ShowDirectoriesAndFiles, $"Команда: {command} не распознана")
            .ValidateDirectoryExists(_menu.Path, "Директория не существует");

        try
        {
            var allItems = GetDirectoryContents();
            DisplayResults(allItems);
        }
        catch (UnauthorizedAccessException)
        {
            throw new UnauthorizedAccessException("Ошибка: Нет доступа к директории");
        }
        catch (PathTooLongException)
        {
            throw new PathTooLongException("Ошибка: Слишком длинный путь");
        }
        catch
        {
            throw new Exception("Неожиданная ошибка, обратитесь к Артёму Красову, он вам поможет");
        }
    }

    private List<string> GetDirectoryContents()
    {
        var namesFilesAndDirectories = new List<string>();

        try
        {
            var directories = Directory.EnumerateDirectories(_menu.Path, "*", SearchOption.TopDirectoryOnly)
                                       .Select(Path.GetFileName)
                                       .OfType<string>();

            var files = Directory.EnumerateFiles(_menu.Path, "*", SearchOption.TopDirectoryOnly)
                                 .Select(Path.GetFileName)
                                 .OfType<string>();

            namesFilesAndDirectories.AddRange(directories);
            namesFilesAndDirectories.AddRange(files);
        }
        catch
        {
            throw new InvalidOperationException("Ошибка при чтении директорий или файло");
        }

        return namesFilesAndDirectories;
    }

    private void DisplayResults(List<string> namesFilesAndDirectories)
    {
        var stringResult = new StringBuilder();

        if (namesFilesAndDirectories.IsEmpty())
        {
            stringResult.Append("Текущая директория пуста");
        }
        else
        {
            for (int i = 0; i < namesFilesAndDirectories.Count - 1; i++)
            {
                stringResult.AppendLine(namesFilesAndDirectories[i]);
            }

            stringResult.Append(namesFilesAndDirectories.Last());
        }

        _menu.Output.Invoke(stringResult.ToString());
    }
}