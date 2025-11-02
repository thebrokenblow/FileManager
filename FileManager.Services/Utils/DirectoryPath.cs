using FileManager.Services.Extensions;

namespace FileManager.Services.Utils;

public class DirectoryPath(IMenu menu)
{
    private readonly IMenu _menu = menu ??
        throw new ArgumentNullException(nameof(menu));

    public string GetDirectoryBelow()
    {
        var pathElementsWithoutLastDirectory = _menu.Path.Split(Path.DirectorySeparatorChar)
                                                         .ToList()
                                                         .RemoveLast();

        var pathWithoutLastDirectory = string.Join(Path.DirectorySeparatorChar, pathElementsWithoutLastDirectory);

        var currentDirectory = Directory.GetCurrentDirectory();
        if (pathWithoutLastDirectory == currentDirectory)
        {
            throw new ArgumentException($"Нельзя выйти за границы дериктории: {menu.Path}");
        }

        return pathWithoutLastDirectory;
    }
}