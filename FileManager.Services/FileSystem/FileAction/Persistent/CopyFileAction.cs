using FileManager.Domain.Entities;
using FileManager.Domain.Entities.Enums;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;

namespace FileManager.Services.FileSystem.FileAction.Persistent;

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

    private readonly CommandValidator commandValidator = new();

    private string? _nameSourceFile;
    private string? _nameDestinationDirectory;

    public void Execute(string command)
    {
        commandValidator
            .ValidateNotEmpty(command, "Команда не может быть пустой")
            .ValidateArgumentsCount(out string[] arguments, command, CountArguments, $"Некорректное количество аргументов: {command}");

        var nameCommand = arguments.First();
        _nameSourceFile = arguments.Second();
        _nameDestinationDirectory = arguments.Third();
        var fullPathSourceFile = Path.Combine(menu.Path, _nameSourceFile);

        commandValidator
            .ValidateCommandName(nameCommand, CommandDictionary.CopyFile, $"Команда: {command} не распознана")
            .ValidateNotEmpty(_nameSourceFile, "Название файла не может быть пустым")
            .ValidatePathSecurity(_nameSourceFile, $"Недопустимое имя файла: {_nameSourceFile}")
            .ValidateFileExists(fullPathSourceFile, $"Не существует файла с названием: {_nameSourceFile}");

        if (_nameDestinationDirectory.Equals(ArgumentMoveFileBelow, StringComparison.CurrentCultureIgnoreCase))
        {
            CopyFileToDirectoryBelow(_nameSourceFile, fullPathSourceFile);
            return;
        }

        var fullPathDestinationDirectory = Path.Combine(menu.Path, _nameDestinationDirectory);
        var fullPathDestinationFile = Path.Combine(fullPathDestinationDirectory, _nameSourceFile);

        commandValidator
            .ValidateNotEmpty(_nameDestinationDirectory, "Название директории не должно быть пустым")
            .ValidatePathSecurity(_nameDestinationDirectory, $"Недопустимое имя директории: {_nameDestinationDirectory}")
            .ValidateDirectoryExists(fullPathDestinationDirectory, $"Не существует директории с названием: {_nameDestinationDirectory}")
            .ValidateFileNotExists(fullPathDestinationFile, $"Файл с названием: {_nameSourceFile} уже существует в директории: {_nameDestinationDirectory}");

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

        commandValidator
            .ValidateFileNotExists(fullPathFileBelow, $"Уже существует файл: {nameSourceFile} на уровне ниже");

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
