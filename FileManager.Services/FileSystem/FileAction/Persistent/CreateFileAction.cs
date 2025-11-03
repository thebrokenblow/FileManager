using FileManager.Domain.Interfaces.UseCases;
using FileManager.Domain.Model;
using FileManager.Services.Exceptions;
using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;

namespace FileManager.Services.FileSystem.FileAction.Persistent;

public class CreateFileAction(
    IMenu menu, 
    IFileUseCase fileUseCase) : IFileSystemAction, IFileSystemPersistentAction
{
    private const int CountArguments = 2;
    private const int FileSizeWhenCreated = 0;

    private readonly IMenu _menu = menu ??
        throw new ArgumentNullException(nameof(menu));

    private readonly IFileUseCase _fileUseCase = fileUseCase ??
        throw new ArgumentNullException(nameof(fileUseCase));

    private readonly CommandValidator commandValidator = new();

    private string? _nameFile;
    private string? _fullPath;

    public void Execute(string command)
    {
        ResetFiled();

        commandValidator
            .ValidateNotEmpty(command, "Команда не может быть пустой")
            .ValidateArgumentsCount(out string[] arguments, command, CountArguments, $"Некорректное количество аргументов: {command}");

        var nameCommand = arguments.First();
        _nameFile = arguments.Second();
        _fullPath = Path.Combine(_menu.Path, _nameFile);

        commandValidator
            .ValidateCommandName(nameCommand, CommandDictionary.CreateFile, $"Команда: {command} не распознана")
            .ValidateNotEmpty(_nameFile, "Имя файла не может быть пустым")
            .ValidatePathSecurity(_nameFile, $"Недопустимое имя файла: {_nameFile}")
            .ValidateFileNotExists(_fullPath, $"Уже существует такой файл: {_nameFile}");

        try
        {
            var file = File.Create(_fullPath);
            file.Dispose();
        }
        catch
        {
            throw new ArgumentException($"Ошибка создания файла");
        }   
    }

    public async Task SaveToDatabaseAsync()
    {
        if (_nameFile is null || _fullPath is null)
        {
            throw new InvalidOperationException("Некорректное поведение системы");
        }

        try
        {
            var dateTimeCreateFile = DateTime.UtcNow;
            var fileModel = new FileModel(
                _nameFile, 
                _fullPath, 
                dateTimeCreateFile, 
                FileSizeWhenCreated);

            await _fileUseCase.CreateFileAsync(fileModel, _menu.UserId);
        }
        catch (Exception ex)
        {
            throw new DatabaseOperationException($"Ошибка при создании файла в БД: {_nameFile}", ex);
        }
    }

    private void ResetFiled()
    {
        _nameFile = null;
        _fullPath = null;
    }
}