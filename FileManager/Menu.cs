using FileManager.Actions;
using FileManager.Actions.DirectoryAction;
using FileManager.Actions.Disck;
using FileManager.Actions.FileAction;
using FileManager.Actions.Other;
using FileManager.Data;
using FileManager.Data.Repositories;
using FileManager.Utils;

namespace FileManager;

public class Menu
{
    public string CurrentPath { get; set; }
    public int UserId { get; private set; } = 1;

    private readonly Dictionary<string, IAction> actionByTitleCommand;

    public const string TitleUserDirectory = "UserDirectory";
    public Menu(string currentPath, FileManagerContext context)
    {
        CurrentPath = currentPath;

        var directoryPath = new DirectoryPath(this);

        var directoryRepository = new DirectoryRepository(context);

        var fileRepository = new FileRepository(context);
        var infoFileRepository = new InfoFileRepository(context);
        
        actionByTitleCommand = new Dictionary<string, IAction>
        {
            { CommandDictionary.ChangeDirectory, new ChangeDirectoryAction(this, directoryPath) },
            { CommandDictionary.MoveToParentDirectory, new ChangeDirectoryAction(this, directoryPath) },
            { CommandDictionary.CreateDirectory, new CreateDirectoryAction(this, directoryRepository) },
            { CommandDictionary.DeleteDirectory, new DeleteDirectoryAction(this, directoryRepository) },
            { CommandDictionary.MoveDirectory, new MoveDirectoriesAction(this, directoryPath, directoryRepository) },

            { CommandDictionary.ShowDiskInfo, new InfoDisckAction() },

            { CommandDictionary.Help, new HelpAction() },

            { CommandDictionary.ShowDirectoriesAndFiles, new ShowDirectoriesAction(this) },
            { CommandDictionary.CopyFile, new CopyFileAction(this, directoryPath, fileRepository) },
            { CommandDictionary.CreateFile, new CreateFileAction(this, fileRepository) },
            { CommandDictionary.MoveFile, new MoveFileAction(this, directoryPath, fileRepository) },
            { CommandDictionary.DeleteFile, new DeleteFileAction(this, fileRepository) },
            { CommandDictionary.ReadFile, new ReadFileAction(this) },
            { CommandDictionary.WriteFile, new WriteFileAction(this, fileRepository, infoFileRepository) },
            { CommandDictionary.DeserializeXml, new DeserializeXmlFileAction(this, context) },
            { CommandDictionary.DeserializeJson, new DeserializeJsonFileAction(this, context) },
            { CommandDictionary.ArchiveZip, new ArchiveZipFilesAction(this, fileRepository) },
            { CommandDictionary.UnarchiveZip, new UnarchiveZipFilesAction(this, context) },
        };
    }

    public void EnterCommand()
    {
        Console.Write($"{CurrentPath}: ");

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
            if (actionByTitleCommand.TryGetValue(titleCommand, out IAction? actioncommand))
            {
                actioncommand.Execute(inputCommand);
            }
            else
            {
                Console.WriteLine($"Отсутствует команда: {inputCommand} воспользуйтесь help");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}