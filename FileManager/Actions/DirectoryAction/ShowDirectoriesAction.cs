using FileManager.Extensions;
using FileManager.Utils;

namespace FileManager.Actions.DirectoryAction;

public class ShowDirectoriesAction(Menu menu) : IAction
{
    public void Execute(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            throw new ArgumentException("Команда не может быть пустой");
        }

        if (!command.Equals(CommandDictionary.ShowDirectoriesAndFiles, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException($"Команда: {command} не распознана");
        }

        var nameDirectories = Directory.GetDirectories(menu.CurrentPath).Select(Path.GetFileName).ToList();
        var nameFiles = Directory.GetFiles(menu.CurrentPath).Select(Path.GetFileName).ToList();

        nameDirectories.AddRange(nameFiles);

        if (nameDirectories.IsEmpty())
        {
            Console.WriteLine("Текукщая директория пуста");
        }
        else
        {
            for (int i = 0; i < nameDirectories.Count; i++)
            {
                Console.WriteLine($"{nameDirectories[i]}");
            }
        }
    }
}