using FileManager.Persistence.Data;
using FileManager.Persistence.Queries;
using FileManager.Persistence.Repositories;
using FileManager.Services;
using FileManager.Services.FileSystem.DirectoryAction.Persistent;
using FileManager.Services.FileSystem.DirectoryAction.Transient;
using FileManager.Services.FileSystem.Disck.WithoutDatabase;
using FileManager.Services.FileSystem.FileAction.Persistent;
using FileManager.Services.FileSystem.FileAction.Transient;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.UI.ViewUI.Components;

namespace FileManager.UI.ViewUI;

public class Menu : IMenu
{
    public int UserId { get; } = 1;
    public string Path { get; private set; }

    public Action<string> Output { get; } 
    public Action<Action<string?>, string> InputText { get; }

    private readonly Dictionary<string, IFileSystemAction> fileSystemActionByNameCommand;

    public const string TitleUserDirectory = "UserDirectory";

    public Menu(string currentPath, FileManagerContext context)
    {
        Path = currentPath;
        Output = Console.WriteLine;
        InputText = InputPanel.Input;

        var directoryPath = new DirectoryPath(this);

        var directoryQueries = new DirectoryQueries(context);
        var operationDirectoryRepository = new OperationDirectoryRepository(context);
        var directoryRepository = new DirectoryRepository(context, operationDirectoryRepository);

        var fileQueries = new FileQueries(context);
        var operationFileRepository = new OperationFileRepository(context);
        var fileRepository = new FileRepository(context, operationFileRepository);

        fileSystemActionByNameCommand = new Dictionary<string, IFileSystemAction>
        {
            { CommandDictionary.ChangeDirectory, new ChangeDirectoryAction(this, directoryPath) },
            { CommandDictionary.MoveToParentDirectory, new ChangeDirectoryAction(this, directoryPath) },
            { CommandDictionary.CreateDirectory, new CreateDirectoryAction(this, directoryRepository) },
            { CommandDictionary.DeleteDirectory, new DeleteDirectoryAction(this, directoryQueries, operationDirectoryRepository) },
            { CommandDictionary.MoveDirectory, new MoveDirectoriesAction(this, directoryQueries, directoryRepository, directoryPath) },

            { CommandDictionary.ShowDiskInfo, new InfoDisckAction(this) },

            //{ CommandDictionary.Help, new HelpAction() },

            { CommandDictionary.ShowDirectoriesAndFiles, new ShowDirectoriesAction(this) },
            { CommandDictionary.CopyFile, new CopyFileAction(this, fileRepository, directoryPath) },
            { CommandDictionary.CreateFile, new CreateFileAction(this, fileRepository) },
            { CommandDictionary.MoveFile, new MoveFileAction(this, fileQueries, fileRepository, directoryPath) },
            { CommandDictionary.DeleteFile, new DeleteFileAction(this, fileQueries, operationFileRepository) },
            { CommandDictionary.ReadFile, new ReadFileAction(this) },
            { CommandDictionary.WriteFile, new WriteFileAction(this, fileQueries, operationFileRepository) },
            { CommandDictionary.DeserializeXml, new DeserializeXmlFileAction(this) },
            { CommandDictionary.DeserializeJson, new DeserializeJsonFileAction(this) },
            { CommandDictionary.ArchiveZip, new ArchiveZipFilesAction(this, fileRepository) },
            { CommandDictionary.UnarchiveZip, new UnarchiveZipFilesAction(this) },
        };
    }

    public async Task EnterCommand()
    {
        Console.Write($"{Path}: ");

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
            if (fileSystemActionByNameCommand.TryGetValue(titleCommand, out IFileSystemAction? actioncommand))
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

    public void ChangePath(string path)
    {
        Path = path;
    }
}