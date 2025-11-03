using FileManager.Domain.Interfaces.UseCases;
using FileManager.Domain.Model;
using FileManager.Services.Exceptions;
using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;

namespace FileManager.Services.FileSystem.FileAction.Persistent;

public class WriteFileAction(
    IMenu menu,
    IFileUseCase fileUseCase) : IFileSystemAction, IFileSystemPersistentAction
{
    private const int CountArguments = 2;

    private readonly IMenu _menu = menu ??
        throw new ArgumentNullException(nameof(menu));

    private readonly CommandValidator commandValidator = new();

    private long? _fileSize;
    private string? _nameFile;
    private string? _fullPathFile;

    public void Execute(string command)
    {
        ResetFiled();

        commandValidator
            .ValidateNotEmpty(command, "Команда не может быть пустой")
            .ValidateArgumentsCount(out string[] arguments, command, CountArguments, $"Некорректное количество аргументов: {command}");

        var nameCommand = arguments.First();
        _nameFile = arguments.Second();
        _fullPathFile = Path.Combine(_menu.Path, _nameFile);

        commandValidator
            .ValidateCommandName(nameCommand, CommandDictionary.WriteFile, $"Команда: {command} не распознана")
            .ValidateNotEmpty(_nameFile, "Имя файла не может быть пустым")
            .ValidatePathSecurity(_nameFile, $"Недопустимое имя файла: {_nameFile}")
            .ValidateFileExists(_fullPathFile, $"Файла с именем: {_nameFile} несуществует");

        var sourceText = File.ReadAllText(_fullPathFile);

        _menu.InputText.Invoke(WriteFile, sourceText);
    }

    public async Task SaveToDatabaseAsync()
    {
        if (_fullPathFile is null || _fileSize is null)
        {
            throw new InvalidOperationException("Некорректное поведение системы");
        }

        try
        {
            var modifyAt = DateTime.UtcNow;
            var modifyFileSizeOperationModel = new ModifyFileSizeOperationModel(
                _fullPathFile,
                _fileSize.Value,
                modifyAt,
                _menu.UserId);

            await fileUseCase.ModifyFileSizeOperationAsync(modifyFileSizeOperationModel);
        }
        catch (Exception ex)
        {
            throw new DatabaseOperationException($"Ошибка базы данных при обновлении размера файла '{_fullPathFile}'", ex);
        }
    }

    private void ResetFiled()
    {
        _fileSize = null;
        _nameFile = null;
        _fullPathFile = null;
    }

    private void WriteFile(string? contents)
    {
        if (_fullPathFile is null)
        {
            throw new InvalidOperationException("Некорректное поведение системы");
        }

        try
        {
            File.WriteAllText(_fullPathFile, contents);
            var fileInfo = new FileInfo(_fullPathFile);
            _fileSize = fileInfo.Length;
        }
        catch
        {
            throw new ArgumentException("Ошибка записи в файла");
        }
    }
}