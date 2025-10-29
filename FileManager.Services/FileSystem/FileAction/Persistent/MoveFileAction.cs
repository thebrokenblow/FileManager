using FileManager.Domain.Interfaces.Queries;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;

namespace FileManager.Services.FileSystem.FileAction.Persistent;

public class MoveFileAction(
    IMenu menu,
    IFileQueries fileQueries,
    IFileRepository fileRepository,
    DirectoryPath directoryPath) : IFileSystemAction, IFileSystemPersistentAction
{
    private const int CountArguments = 3;
    private const string ArgumentMoveFileBelow = "-l";

    private readonly IMenu _menu = menu ??
        throw new ArgumentNullException(nameof(menu));

    private readonly IFileQueries _fileQueries = fileQueries ??
        throw new ArgumentNullException(nameof(fileQueries));

    private readonly IFileRepository _fileRepository = fileRepository ??
        throw new ArgumentNullException(nameof(fileRepository));

    private readonly CommandValidator commandValidator = new();

    private string? _fullPathSourceFile;
    private string? _fullNameDestinationDirectoryFile;

    public void Execute(string command)
    {
        commandValidator
            .ValidateNotEmpty(command, "Команда не может быть пустой")
            .ValidateArgumentsCount(out string[] arguments, command, CountArguments, $"Некорректное количество аргументов: {command}");

        var nameCommand = arguments.First();
        var nameSourceFile = arguments.Second();
        var nameDestinationDirectory = arguments.Third();
        _fullPathSourceFile = Path.Combine(_menu.Path, nameSourceFile);
        var fullPathDestinationDirectory = Path.Combine(menu.Path, nameDestinationDirectory);

        commandValidator
            .ValidateCommandName(nameCommand, CommandDictionary.MoveFile, $"Команда: {command} не распознана")
            .ValidateNotEmpty(nameSourceFile, "Название файла не может быть пустым")
            .ValidateFileExists(_fullPathSourceFile, $"Не существует файла с названием: {nameSourceFile}")
            .ValidatePathSecurity(nameSourceFile, $"Недопустимое имя файла: {nameSourceFile}");

        if (nameDestinationDirectory.Equals(ArgumentMoveFileBelow, StringComparison.CurrentCultureIgnoreCase))
        {
            MoveFileToDirectoryBelow(nameSourceFile, _fullPathSourceFile);
            return;
        }

        commandValidator
            .ValidateNotEmpty(nameDestinationDirectory, "Название директории не может быть пустым")
            .ValidateDirectoryExists(fullPathDestinationDirectory, $"Не существует директории с названием: {nameDestinationDirectory}");

        _fullNameDestinationDirectoryFile = Path.Combine(fullPathDestinationDirectory, nameSourceFile);

        MoveFile(_fullPathSourceFile, _fullNameDestinationDirectoryFile);
    }

    public async Task SaveToDatabaseAsync()
    {
        if (_fullPathSourceFile is null || _fullNameDestinationDirectoryFile is null)
        {
            throw new InvalidOperationException("Некорректное поведение системы");
        }

        var infoFile = await _fileQueries.GetByLocationAsync(_fullPathSourceFile) ?? 
                                throw new Exception("Отсутствует соответствующая запись в базе данных");
            
        infoFile.Location = _fullNameDestinationDirectoryFile;

        await _fileRepository.UpdateAsync(infoFile);
    }

    private void MoveFileToDirectoryBelow(string nameSourceFile, string fullPathSourceFile)
    {
        var directoryBelow = directoryPath.GetDirectoryBelow();
        _fullNameDestinationDirectoryFile = Path.Combine(directoryBelow, nameSourceFile);

        commandValidator
            .ValidateFileNotExists(_fullNameDestinationDirectoryFile, $"Уже существует файл: {nameSourceFile} на уровне ниже");

        MoveFile(fullPathSourceFile, _fullNameDestinationDirectoryFile);
    }

    private static void MoveFile(string sourceFileName, string destFileName)
    {
        try
        {
            File.Move(sourceFileName, destFileName);
        }
        catch
        {
            throw new ArgumentException($"Ошибка перемещения файла");
        }
    }
}