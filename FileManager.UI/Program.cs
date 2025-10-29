using FileManager.Persistence.Data;
using FileManager.UI.ViewUI;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Configurations\\appsettings.json", optional: false, reloadOnChange: true)
            .Build();

string connectionString = configuration.GetConnectionString("DbConnection")
                                ?? throw new InvalidOperationException("Connection string not found");

var fileManagerContextFactory = new FileManagerContextFactory(connectionString);
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
    await menu.EnterCommand();
}