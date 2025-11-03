using FileManager.Services;
using FileManager.Services.FileSystem.DirectoryAction.Persistent;
using FileManager.Services.FileSystem.DirectoryAction.Transient;
using FileManager.Services.FileSystem.Disck.Transient;
using FileManager.Services.FileSystem.FileAction.Persistent;
using FileManager.Services.FileSystem.FileAction.Transient;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.FileSystem.Other.Transient;
using FileManager.Services.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager.UI.ViewUI;

public class MenuHandler
{
    private readonly IMenu _menu;
    private Dictionary<string, IFileSystemAction> _fileSystemActionByNameCommand = [];

    public MenuHandler(IMenu menu, IServiceProvider serviceProvider)
    {
        _menu = menu;
        ConfigurationCommands(serviceProvider);
    }

    public async Task EnterCommand()
    {
        Console.Write($"{_menu.Path}: ");

        var inputCommand = Console.ReadLine();

        if (string.IsNullOrEmpty(inputCommand))
        {
            Console.WriteLine("Команда не распознана, повторите её");
            return;
        }

        var commandAndArguments = inputCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var titleCommand = commandAndArguments.First()
                                              .ToLower();
        try
        {
            if (_fileSystemActionByNameCommand.TryGetValue(titleCommand, out IFileSystemAction? actioncommand))
            {
                actioncommand.Execute(inputCommand);
            }
            else
            {
                Console.WriteLine($"Отсутствует команда: {inputCommand} воспользуйтесь help");
            }

            if (actioncommand is IFileSystemPersistentAction fileSystemPersistentAction)
            {
                await fileSystemPersistentAction.SaveToDatabaseAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void ConfigurationCommands(IServiceProvider serviceProvider)
    {
        var createDirectoryAction = serviceProvider.GetRequiredService<CreateDirectoryAction>();
        var deleteDirectoryAction = serviceProvider.GetRequiredService<DeleteDirectoryAction>();
        var moveDirectoriesAction = serviceProvider.GetRequiredService<MoveDirectoriesAction>();
        var changeDirectoryAction = serviceProvider.GetRequiredService<ChangeDirectoryAction>();
        var showDirectoriesAction = serviceProvider.GetRequiredService<ShowDirectoriesAction>();

        var infoDisckAction = serviceProvider.GetRequiredService<InfoDisckAction>();

        var helpAction = serviceProvider.GetRequiredService<HelpAction>();

        var archiveZipFilesAction = serviceProvider.GetRequiredService<ArchiveZipFilesAction>();
        var copyFileAction = serviceProvider.GetRequiredService<CopyFileAction>();
        var createFileAction = serviceProvider.GetRequiredService<CreateFileAction>();
        var deleteFileAction = serviceProvider.GetRequiredService<DeleteFileAction>();
        var moveFileAction = serviceProvider.GetRequiredService<MoveFileAction>();
        var writeFileAction = serviceProvider.GetRequiredService<WriteFileAction>();
        var deserializeJsonFileAction = serviceProvider.GetRequiredService<DeserializeJsonFileAction>();
        var deserializeXmlFileAction = serviceProvider.GetRequiredService<DeserializeXmlFileAction>();
        var readFileAction = serviceProvider.GetRequiredService<ReadFileAction>();
        var unarchiveZipFilesAction = serviceProvider.GetRequiredService<UnarchiveZipFilesAction>();

        _fileSystemActionByNameCommand.Add(CommandDictionary.ChangeDirectory, changeDirectoryAction);
        _fileSystemActionByNameCommand.Add(CommandDictionary.MoveToParentDirectory, changeDirectoryAction);
        _fileSystemActionByNameCommand.Add(CommandDictionary.CreateDirectory, createDirectoryAction);
        _fileSystemActionByNameCommand.Add(CommandDictionary.DeleteDirectory, deleteDirectoryAction);
        _fileSystemActionByNameCommand.Add(CommandDictionary.MoveDirectory, moveDirectoriesAction);

        _fileSystemActionByNameCommand.Add(CommandDictionary.ShowDiskInfo, infoDisckAction);

        _fileSystemActionByNameCommand.Add(CommandDictionary.Help, helpAction);

        _fileSystemActionByNameCommand.Add(CommandDictionary.ShowDirectoriesAndFiles, showDirectoriesAction);
        _fileSystemActionByNameCommand.Add(CommandDictionary.CopyFile, copyFileAction);
        _fileSystemActionByNameCommand.Add(CommandDictionary.CreateFile, createFileAction);
        _fileSystemActionByNameCommand.Add(CommandDictionary.MoveFile, moveFileAction);
        _fileSystemActionByNameCommand.Add(CommandDictionary.DeleteFile, deleteFileAction);
        _fileSystemActionByNameCommand.Add(CommandDictionary.ReadFile, readFileAction);
        _fileSystemActionByNameCommand.Add(CommandDictionary.WriteFile, writeFileAction);
        _fileSystemActionByNameCommand.Add(CommandDictionary.DeserializeXml, deserializeXmlFileAction);
        _fileSystemActionByNameCommand.Add(CommandDictionary.DeserializeJson, deserializeJsonFileAction);
        _fileSystemActionByNameCommand.Add(CommandDictionary.ArchiveZip, archiveZipFilesAction);
        _fileSystemActionByNameCommand.Add(CommandDictionary.UnarchiveZip, unarchiveZipFilesAction);
    }
}
