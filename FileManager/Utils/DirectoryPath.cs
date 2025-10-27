using FileManager.Extensions;

namespace FileManager.Utils;

public class DirectoryPath(Menu menu)
{
    public string GetDirectoryBelow()
    {
        var pathElementsWithoutLastDirectory = menu.CurrentPath.Split('\\')
                                                               .ToList()
                                                               .RemoveLast();

        var pathWithoutLastDirectory = string.Join('\\', pathElementsWithoutLastDirectory);

        var currentDirectory = Directory.GetCurrentDirectory();
        if (pathWithoutLastDirectory == currentDirectory)
        {
            throw new ArgumentException($"Нельзя выйти за границы дериктории: {menu.CurrentPath}");
        }

        return pathWithoutLastDirectory;
    }
}
