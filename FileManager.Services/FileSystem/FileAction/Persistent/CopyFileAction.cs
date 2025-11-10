using FileManager.Domain.Interfaces.UseCases;
using FileManager.Domain.Model;
using FileManager.Services.Exceptions;
using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;

namespace FileManager.Services.FileSystem.FileAction.Persistent;

public class CopyFileAction(
    IMenu menu,
    IFileUseCase fileUseCase,
    DirectoryPath directoryPath) : IFileSystemCommand, IFileSystemPersistent
{
    private const int CountArguments = 3;
    public const string ArgumentMoveFileBelow = "-l";

    private readonly IMenu _menu = menu ?? 
        throw new ArgumentNullException(nameof(menu));

    private readonly IFileUseCase _fileUseCase = fileUseCase ??
        throw new ArgumentNullException(nameof(fileUseCase));

    private readonly DirectoryPath _directoryPath = directoryPath ??
        throw new ArgumentNullException(nameof(directoryPath));

    private readonly CommandValidator commandValidator = new();

    private long? _fileSize;
    private string? _nameSourceFile;
    private string? _fullPathSourceFile;
    private string? _fullPathDestinationFile;

    public void Execute(string command)
    {
        ResetFiled();

        commandValidator
            .ValidateNotEmpty(command, "Команда не может быть пустой")
            .ValidateArgumentsCount(out string[] arguments, command, CountArguments, $"Некорректное количество аргументов: {command}");

        var nameCommand = arguments.First();
        _nameSourceFile = arguments.Second();
        var nameDestinationDirectory = arguments.Third();

        _fullPathSourceFile = Path.Combine(menu.Path, _nameSourceFile);

        commandValidator
            .ValidateCommandName(nameCommand, CommandDictionary.CopyFile, $"Команда: {command} не распознана")
            .ValidateNotEmpty(_nameSourceFile, "Название файла не может быть пустым")
            .ValidatePathSecurity(_nameSourceFile, $"Недопустимое имя файла: {_nameSourceFile}")
            .ValidateFileExists(_fullPathSourceFile, $"Не существует файла с названием: {_nameSourceFile}");

        if (nameDestinationDirectory.Equals(ArgumentMoveFileBelow, StringComparison.CurrentCultureIgnoreCase))
        {
            CopyFileToDirectoryBelow(_nameSourceFile, _fullPathSourceFile);
            return;
        }

        var fullPathDestinationDirectory = Path.Combine(menu.Path, nameDestinationDirectory);
        _fullPathDestinationFile = Path.Combine(fullPathDestinationDirectory, _nameSourceFile);

        commandValidator
            .ValidateNotEmpty(nameDestinationDirectory, "Название директории не должно быть пустым")
            .ValidatePathSecurity(nameDestinationDirectory, $"Недопустимое имя директории: {nameDestinationDirectory}")
            .ValidateDirectoryExists(fullPathDestinationDirectory, $"Не существует директории с названием: {nameDestinationDirectory}")
            .ValidateFileNotExists(_fullPathDestinationFile, $"Файл с названием: {_nameSourceFile} уже существует в директории: {nameDestinationDirectory}");

        CopyFile(_fullPathSourceFile, _fullPathDestinationFile);
    }

    public async Task SaveToDatabaseAsync()
    {
        if (_fileSize is null ||
            _nameSourceFile is null ||
            _fullPathSourceFile is null || 
            _fullPathDestinationFile is null)
        {
            throw new InvalidOperationException("Некорректное поведение системы");
        }

        try
        {
            var dateTimeCopyFile = DateTime.UtcNow;

            var fileModel = new FileModel(
                _nameSourceFile,
                _fullPathDestinationFile,
                dateTimeCopyFile,
                _fileSize.Value);

            await _fileUseCase.CreateAsync(fileModel, _menu.UserId);
        }
        catch (Exception ex)
        {
            throw new DatabaseOperationException($"Ошибка базы данных при добавлении информации о файле '{_nameSourceFile}'", ex);
        }
    }

    private void CopyFileToDirectoryBelow(string nameSourceFile, string fullPathSourceFile)
    {
        var directoryBelow = _directoryPath.GetDirectoryBelow();
        var fullPathFileBelow = Path.Combine(directoryBelow, nameSourceFile);

        commandValidator
            .ValidateFileNotExists(fullPathFileBelow, $"Уже существует файл: {nameSourceFile} на уровне ниже");

        CopyFile(fullPathSourceFile, fullPathFileBelow);
    }

    private void CopyFile(string sourceFileName, string destFileName)
    {
        try
        {
            File.Copy(sourceFileName, destFileName);
            var fileInfo = new FileInfo(destFileName);
            _fileSize = fileInfo.Length;
        }
        catch
        {
            throw new ArgumentException($"Ошибка перемещения файла");
        }
    }

    private void ResetFiled()
    {
        _fileSize = null;
        _nameSourceFile = null;
        _fullPathSourceFile = null;
        _fullPathDestinationFile = null;
    }
}
