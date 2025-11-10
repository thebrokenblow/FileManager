using FileManager.Services.FileSystem.DirectoryAction.Persistent;
using FileManager.Services.FileSystem.FileAction.Persistent;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;

namespace FileManager.Services.FileSystem.Other.Transient;

/// <summary>
/// Команда для отображения справочной информации по доступным командам
/// </summary>
public class HelpAction(IMenu menu) : IFileSystemCommand
{
    private readonly IMenu _menu = menu ?? 
        throw new ArgumentNullException(nameof(menu));

    private readonly CommandValidator commandValidator = new();

    public void Execute(string command)
    {
        commandValidator
            .ValidateNotEmpty(command, "Команда не может быть пустой")
            .ValidateCommandName(command.Trim(), CommandDictionary.Help, $"Команда: {command} не распознана");

        DisplayHelp();
    }

    private void DisplayHelp()
    {
        var helpInfo = new HelpInfo
        {
            Title = "СПРАВОЧНИК КОМАНД",
            TitleNameCommand = "Команда",
            TitleUsageCommand = "Использование",
            TitleDescriptionCommand = "Описание",
            TitleDirectoryChapter = "ДИРЕКТОРИИ",
            HelpDirectoryCommands = CreateDirectoryCommands(),
            TitleFileChapter = "ФАЙЛЫ",
            HelpFileCommands = CreateFileCommands(),
            TitleUtilityChapter = "СЕРВИС",
            HelpUtilityCommands = CreateUtilityCommands(),
        };

        _menu.OutputHelpPanel.Invoke(helpInfo);
    }

    private static List<HelpCommand> CreateDirectoryCommands()
    {
        var helpDirectoryCommands = new List<HelpCommand>()
        {
            new()
            {
                Name = CommandDictionary.ChangeDirectory,
                Usage = $"{CommandDictionary.ChangeDirectory} <директория>",
                Description = "Переход в указанную директорию"
            },
            new()
            {
                Name = CommandDictionary.MoveToParentDirectory,
                Usage = CommandDictionary.MoveToParentDirectory,
                Description = "Переход в родительскую директорию"
            },
            new()
            {
                Name = CommandDictionary.CreateDirectory,
                Usage = $"{CommandDictionary.CreateDirectory} <имя>",
                Description = "Создание новой директории"
            },
            new()
            {
                Name = CommandDictionary.DeleteDirectory,
                Usage = $"{CommandDictionary.DeleteDirectory} <имя>",
                Description = "Удаление директории"
            },
            new()
            {
                Name = CommandDictionary.MoveDirectory,
                Usage = $"{CommandDictionary.MoveDirectory} <источник> <цель>",
                Description = "Перемещение директории"
            },
            new()
            {
                Name = CommandDictionary.MoveDirectory,
                Usage = $"{CommandDictionary.MoveFile} <источник> {MoveDirectoriesAction.ArgumentMoveDirectoryBelow}",
                Description = "Перемещение директории на уровень ниже"
            },
            new()
            {
                Name = CommandDictionary.ShowDirectoriesAndFiles,
                Usage = CommandDictionary.ShowDirectoriesAndFiles,
                Description = "Показать содержимое текущей директории"
            }
        };

        return helpDirectoryCommands;
    }

    private static List<HelpCommand> CreateFileCommands()
    {
        var helpFileCommands = new List<HelpCommand>()
        {
            new()
            {
                Name = CommandDictionary.CopyFile,
                Usage = $"{CommandDictionary.CopyFile} <файл> <директория>",
                Description = "Копирование файла в директорию"
            },
            new()
            {
                Name = CommandDictionary.CopyFile,
                Usage = $"{CommandDictionary.CopyFile} <файл> {CopyFileAction.ArgumentMoveFileBelow}",
                Description = "Копирование файла на уровень ниже"
            },
            new()
            {
                Name = CommandDictionary.CreateFile,
                Usage = $"{CommandDictionary.CreateFile} <имя>",
                Description = "Создание нового файла"
            },
            new()
            {
                Name = CommandDictionary.DeleteFile,
                Usage = $"{CommandDictionary.DeleteFile} <имя>",
                Description = "Удаление файла"
            },
            new()
            {
                Name = CommandDictionary.ReadFile,
                Usage = $"{CommandDictionary.ReadFile} <имя>",
                Description = "Чтение и вывод содержимого файла"
            },
            new()
            {
                Name = CommandDictionary.WriteFile,
                Usage = $"{CommandDictionary.WriteFile} <имя>",
                Description = "Запись текста в файл (интерактивно)"
            },
            new()
            {
                Name = CommandDictionary.MoveFile,
                Usage = $"{CommandDictionary.MoveFile} <файл> <директория>",
                Description = "Перемещение файла в директорию"
            },
            new()
            {
                Name = CommandDictionary.MoveFile,
                Usage = $"{CommandDictionary.MoveFile} <файл> {MoveFileAction.ArgumentMoveFileBelow}",
                Description = "Перемещение файла на уровень ниже"
            }
        };

        return helpFileCommands;
    }

    private static List<HelpCommand> CreateUtilityCommands()
    {
        var helpUtilityCommands = new List<HelpCommand>()
        {
            new()
            {
                Name = CommandDictionary.DeserializeXml,
                Usage = $"{CommandDictionary.DeserializeXml} <файл>",
                Description = "Десериализация XML файла"
            },
            new()
            {
                Name = CommandDictionary.DeserializeJson,
                Usage = $"{CommandDictionary.DeserializeJson} <файл>",
                Description = "Десериализация JSON файла"
            },
            new()
            {
                Name = CommandDictionary.ArchiveZip,
                Usage = $"{CommandDictionary.ArchiveZip} <архив> <файлы...>",
                Description = "Создание ZIP архива"
            },
            new()
            {
                Name = CommandDictionary.UnarchiveZip,
                Usage = $"{CommandDictionary.UnarchiveZip} <архив>",
                Description = "Распаковка ZIP архива"
            },
            new()
            {
                Name = CommandDictionary.ShowDiskInfo,
                Usage = CommandDictionary.ShowDiskInfo,
                Description = "Информация о дисковых накопителях"
            },
            new()
            {
                Name = CommandDictionary.Help,
                Usage = CommandDictionary.Help,
                Description = "Показать этот справочник"
            }
        };

        return helpUtilityCommands;
    }
}