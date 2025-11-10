using FileManager.Domain.Interfaces.UseCases;
using FileManager.Domain.Model;
using FileManager.Services.Exceptions;
using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;

namespace FileManager.Services.FileSystem.FileAction.Persistent;

public class MoveFileAction(
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

    private string? _fullPathSourceFile;
    private string? _fullNameDestinationDirectoryFile;

    public void Execute(string command)
    {
        ResetFiled();

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

        try
        {
            var moveFileModel = new MoveFileModel(
                _fullPathSourceFile, 
                _fullNameDestinationDirectoryFile,
                DateTime.UtcNow);

            await _fileUseCase.MoveAsync(moveFileModel, _menu.UserId);
        }
        catch (Exception ex)
        {
            throw new DatabaseOperationException($"Ошибка базы данных при обновлении местоположения файла '{_fullPathSourceFile}'", ex);
        }
    }

    private void MoveFileToDirectoryBelow(string nameSourceFile, string fullPathSourceFile)
    {
        var directoryBelow = _directoryPath.GetDirectoryBelow();
        _fullNameDestinationDirectoryFile = Path.Combine(directoryBelow, nameSourceFile);

        commandValidator
            .ValidateFileNotExists(_fullNameDestinationDirectoryFile, $"Уже существует файл: {nameSourceFile} на уровне ниже");

        MoveFile(fullPathSourceFile, _fullNameDestinationDirectoryFile);
    }

    private void ResetFiled()
    {
        _fullPathSourceFile = null;
        _fullNameDestinationDirectoryFile = null;
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