using FileManager.Application.Extensions;
using FileManager.Application.FileSystem.Interfaces;
using FileManager.Application.Utils;
using FileManager.Domain.Entities;
using FileManager.Domain.Entities.Enums;
using FileManager.Domain.Interfaces.Repositories;

namespace FileManager.Application.FileSystem.FileAction.Persistent;

public class CopyFileAction(
    IMenu menu,
    IFileRepository fileRepository,
    DirectoryPath directoryPath) : IFileSystemAction, IFileSystemPersistentAction
{
    private const int CountArguments = 3;
    private const string ArgumentMoveFileBelow = "-l";

    private readonly IMenu _menu = menu ?? 
        throw new ArgumentNullException(nameof(menu));

    private readonly IFileRepository _fileRepository = fileRepository ??
        throw new ArgumentNullException(nameof(fileRepository));

    private string? _nameSourceFile;
    private string? _nameDestinationDirectory;

    public void Execute(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            throw new ArgumentException("Команда не может быть пустой");
        }

        var arguments = command.Split(WhitespaceCharsDictionary.AllWhitespace, StringSplitOptions.RemoveEmptyEntries);
        if (arguments.Length != CountArguments)
        {
            throw new ArgumentException($"Некорректное количество аргументов: {command}");
        }

        var nameCommand = arguments.First();
        if (!nameCommand.Equals(CommandDictionary.CopyFile, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException($"Команда: {command} не распознана");
        }

        _nameSourceFile = arguments.Second();
        if (string.IsNullOrWhiteSpace(_nameSourceFile))
        {
            throw new ArgumentException("Название файла не может быть пустым");
        }

        if (PathSecurity.IsPathTraversal(_nameSourceFile))
        {
            throw new ArgumentException($"Недопустимое имя файла: {_nameSourceFile}");
        }

        var fullPathSourceFile = Path.Combine(menu.Path, _nameSourceFile);
        if (!File.Exists(fullPathSourceFile))
        {
            throw new ArgumentException($"Не существует файла с названием: {_nameSourceFile}");
        }

        _nameDestinationDirectory = arguments.Third();
        if (_nameDestinationDirectory.Equals(ArgumentMoveFileBelow, StringComparison.CurrentCultureIgnoreCase))
        {
            CopyFileToDirectoryBelow(_nameSourceFile, fullPathSourceFile);
            return;
        }

        if (string.IsNullOrWhiteSpace(_nameDestinationDirectory))
        {
            throw new ArgumentException("Название директории не должно быть пустым");
        }

        if (PathSecurity.IsPathTraversal(_nameDestinationDirectory))
        {
            throw new ArgumentException($"Недопустимое имя директории: {_nameDestinationDirectory}");
        }

        var fullPathDestinationDirectory = Path.Combine(menu.Path, _nameDestinationDirectory);
        if (!Directory.Exists(fullPathDestinationDirectory))
        {
            throw new ArgumentException($"Не существует директории с названием: {_nameDestinationDirectory}");
        }

        var fullPathDestinationFile = Path.Combine(fullPathDestinationDirectory, _nameSourceFile);
        if (Directory.Exists(fullPathDestinationFile))
        {
            throw new ArgumentException($"Файл с названием: {_nameSourceFile} уже существует в директории: {_nameDestinationDirectory}");
        }

        CopyFile(fullPathSourceFile, fullPathDestinationFile);
    }

    public async Task SaveToDatabaseAsync()
    {
        if (_nameSourceFile is null || _nameDestinationDirectory is null)
        {
            throw new Exception();    
        }

        var dateTimeCreateArchive = DateTime.UtcNow;

        var infoFile = new InfoFile
        {
            Filename = _nameSourceFile,
            Location = _nameDestinationDirectory,
            CreatedAt = dateTimeCreateArchive,
            Size = 0,
            UserId = _menu.UserId
        };

        var operationFile = new OperationFile
        {
            OperationType = OperationTypeFile.Create,
            ExecutedAt = dateTimeCreateArchive,
            FileId = infoFile.Id,
            UserId = infoFile.UserId
        };

        await _fileRepository.AddAsync(infoFile, operationFile);
    }

    private void CopyFileToDirectoryBelow(string nameSourceFile, string fullPathSourceFile)
    {
        var directoryBelow = directoryPath.GetDirectoryBelow();
        var fullPathFileBelow = Path.Combine(directoryBelow, nameSourceFile);

        if (File.Exists(fullPathFileBelow))
        {
            throw new ArgumentException($"Уже существует файл: {nameSourceFile} на уровне ниже");
        }

        CopyFile(fullPathSourceFile, fullPathFileBelow);
    }

    private static void CopyFile(string sourceFileName, string destFileName)
    {
        try
        {
            File.Copy(sourceFileName, destFileName);
        }
        catch
        {
            throw new ArgumentException($"Ошибка перемещения деректории");
        }
    }
}
