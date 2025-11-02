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
    private readonly Dictionary<string, IFileSystemAction> _fileSystemActionByNameCommand;

    public MenuHandler(IMenu menu, IServiceProvider serviceProvider)
    {
        _menu = menu;

        var createDirectoryAction = serviceProvider.GetRequiredService<CreateDirectoryAction>();
        var deleteDirectoryAction = serviceProvider.GetRequiredService<DeleteDirectoryAction>();
        var moveDirectoriesAction = serviceProvider.GetRequiredService<MoveDirectoriesAction>();

        var changeDirectoryAction = serviceProvider.GetRequiredService<ChangeDirectoryAction>();
        var showDirectoriesAction = serviceProvider.GetRequiredService<ShowDirectoriesAction>();

        var infoDisckAction = serviceProvider.GetRequiredService<InfoDisckAction>();

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

        var helpAction = serviceProvider.GetRequiredService<HelpAction>();


        _fileSystemActionByNameCommand = new Dictionary<string, IFileSystemAction>
        {
            { CommandDictionary.ChangeDirectory, changeDirectoryAction },
            { CommandDictionary.MoveToParentDirectory, changeDirectoryAction },
            { CommandDictionary.CreateDirectory, createDirectoryAction },
            { CommandDictionary.DeleteDirectory, deleteDirectoryAction },
            { CommandDictionary.MoveDirectory, moveDirectoriesAction },

            { CommandDictionary.ShowDiskInfo, infoDisckAction },

            { CommandDictionary.Help, helpAction },

            { CommandDictionary.ShowDirectoriesAndFiles, showDirectoriesAction },
            { CommandDictionary.CopyFile, copyFileAction },
            { CommandDictionary.CreateFile, createFileAction },
            { CommandDictionary.MoveFile, moveFileAction },
            { CommandDictionary.DeleteFile, deleteFileAction },
            { CommandDictionary.ReadFile, readFileAction },
            { CommandDictionary.WriteFile, writeFileAction },
            { CommandDictionary.DeserializeXml, deserializeXmlFileAction },
            { CommandDictionary.DeserializeJson, deserializeJsonFileAction },
            { CommandDictionary.ArchiveZip, archiveZipFilesAction },
            { CommandDictionary.UnarchiveZip, unarchiveZipFilesAction },
        };
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
}
