using FileManager.Application.Extensions;
using FileManager.Application.FileSystem.Interfaces;
using FileManager.Application.Utils;
using FileManager.Domain.Interfaces.Queries;
using FileManager.Domain.Interfaces.Repositories;

namespace FileManager.Application.FileSystem.FileAction.Persistent;

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

    private string? _fullPathSourceFile;
    private string? _fullNameDestinationDirectoryFile;

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
        if (!nameCommand.Equals(CommandDictionary.MoveFile, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException($"Команда: {command} не распознана");
        }

        var nameSourceFile = arguments.Second();
        if (string.IsNullOrWhiteSpace(nameSourceFile))
        {
            throw new ArgumentException("Название файла не может быть пустым");
        }

        _fullPathSourceFile = Path.Combine(_menu.Path, nameSourceFile);
        if (!File.Exists(_fullPathSourceFile))
        {
            throw new ArgumentException($"Не существует файла с названием: {nameSourceFile}");
        }

        if (PathSecurity.IsPathTraversal(nameSourceFile))
        {
            throw new ArgumentException($"Недопустимое имя файла: {nameSourceFile}");
        }

        var nameDestinationDirectory = arguments.Third();
        if (nameDestinationDirectory.Equals(ArgumentMoveFileBelow, StringComparison.CurrentCultureIgnoreCase))
        {
            MoveFileToDirectoryBelow(nameSourceFile, _fullPathSourceFile);
            return;
        }

        if (string.IsNullOrWhiteSpace(nameDestinationDirectory))
        {
            throw new ArgumentException("Название директории не может быть пустым");
        }

        var fullPathDestinationDirectory = Path.Combine(menu.Path, nameDestinationDirectory);
        if (!Directory.Exists(fullPathDestinationDirectory))
        {
            throw new ArgumentException($"Не существует директории с названием: {nameDestinationDirectory}");
        }

        _fullNameDestinationDirectoryFile = Path.Combine(fullPathDestinationDirectory, nameSourceFile);

        MoveFile(_fullPathSourceFile, _fullNameDestinationDirectoryFile);
    }

    public async Task SaveToDatabaseAsync()
    {
        if (_fullPathSourceFile is null || _fullNameDestinationDirectoryFile is null)
        {
            throw new Exception();
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

        if (File.Exists(_fullNameDestinationDirectoryFile))
        {
            throw new ArgumentException($"Уже существует файл: {nameSourceFile} на уровне ниже");
        }

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