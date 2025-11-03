using FileManager.Persistence.Extensions;
using FileManager.Services;
using FileManager.Services.Extensions;
using FileManager.Services.Utils;
using FileManager.UI.ViewUI;
using FileManager.UI.ViewUI.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Configurations\\appsettings.json", optional: false, reloadOnChange: true)
            .Build();

string connectionString = configuration.GetConnectionString("DbConnection")
                                ?? throw new InvalidOperationException("Connection string not found");

var currentPath = Directory.GetCurrentDirectory();
var fullPathUserDirectory = Path.Combine(currentPath, Menu.TitleUserDirectory);

if (!Directory.Exists(fullPathUserDirectory))
{
    Directory.CreateDirectory(fullPathUserDirectory);
}

services.AddScoped<DirectoryPath>();
services.AddScoped<IMenu>(provider => new Menu
{
    UserId = 1,
    Path = fullPathUserDirectory,
    Output = Console.WriteLine,
    OutputHelpPanel = HelpPanel.Output,
    InputText = InputPanel.Input
});

services.AddPersistenceServices(connectionString);
services.AddApplicationServices();

using var serviceProvider = services.BuildServiceProvider();
var menu = serviceProvider.GetRequiredService<IMenu>();

var menuHandler = new MenuHandler(menu, serviceProvider);

while (true)
{
    await menuHandler.EnterCommand();
}