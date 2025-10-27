using FileManager;
using FileManager.Data;

var fileManagerContextFactory = new FileManagerContextFactory();
var context = fileManagerContextFactory.CreateDbContext(args);

var currentPath = Directory.GetCurrentDirectory();
var fullPathUserDirectory = Path.Combine(currentPath, Menu.TitleUserDirectory);

if (!Directory.Exists(fullPathUserDirectory))
{
    Directory.CreateDirectory(fullPathUserDirectory);
}

var menu = new Menu(fullPathUserDirectory, context);

Console.WriteLine("File Manager started. Type 'help' for commands.");

while (true)
{
    menu.EnterCommand();
}