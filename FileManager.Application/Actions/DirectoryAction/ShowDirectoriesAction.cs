using FileManager.Application.Actions.Interfaces;
using FileManager.Application.Extensions;
using FileManager.Application.Utils;
using System.Text;

namespace FileManager.Application.Actions.DirectoryAction;

public class ShowDirectoriesAction(IMenu menu) : IAction
{
    private readonly IMenu _menu = menu ?? throw new ArgumentNullException(nameof(menu));

    public void Execute(string command)
    {
        if (_menu.Output is null)
        {
            throw new ArgumentException("Не установлени Output метод");
        }

        if (string.IsNullOrWhiteSpace(command))
        {
            throw new ArgumentException("Команда не может быть пустой");
        }

        if (!command.Equals(CommandDictionary.ShowDirectoriesAndFiles, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException($"Команда: {command} не распознана");
        }

        if (!Directory.Exists(_menu.Path))
        {
            throw new ArgumentException("Директория не существует");
        }

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
            throw new InvalidOperationException("Ошибка при чтении директории");
        }

        return namesFilesAndDirectories;
    }

    private void DisplayResults(List<string> items)
    {
        var result = new StringBuilder();

        if (items.IsEmpty())
        {
            result.AppendLine("Текущая директория пуста");
        }
        else
        {
            foreach (var item in items.OrderBy(x => x))
            {
                result.AppendLine(item);
            }
        }

        _menu.Output.Invoke(result.ToString());
    }
}