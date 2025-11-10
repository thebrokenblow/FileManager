using FileManager.Domain.Entities;
using FileManager.Domain.Entities.Enums;
using FileManager.Domain.Interfaces.Queries;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Services.Exceptions;
using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;

namespace FileManager.Services.FileSystem.FileAction.Persistent;

public class DeleteFileAction(
    IMenu menu,
    IFileQueries fileQueries,
    IOperationFileRepository operationFileRepository) : IFileSystemCommand, IFileSystemPersistent
{
    private const int CountArguments = 2;

    private readonly IMenu _menu = menu ??
        throw new ArgumentNullException(nameof(menu));

    private readonly IFileQueries _fileQueries = fileQueries ??
        throw new ArgumentNullException(nameof(fileQueries));

    private readonly IOperationFileRepository _operationFileRepository = operationFileRepository ??
        throw new ArgumentNullException(nameof(operationFileRepository));

    private readonly CommandValidator commandValidator = new();

    private string? _fullPath;

    public void Execute(string command)
    {
        ResetFiled();

        commandValidator
            .ValidateNotEmpty(command, "Команда не может быть пустой")
            .ValidateArgumentsCount(out string[] arguments, command, CountArguments, $"Некорректное количество аргументов: {command}");

        var nameCommand = arguments.First();
        var nameFile = arguments.Second();
        _fullPath = Path.Combine(menu.Path, nameFile);

        commandValidator
            .ValidateCommandName(nameCommand, CommandDictionary.DeleteFile, $"Команда: {command} не распознана")
            .ValidateNotEmpty(nameFile, "Имя файла не может быть пустым")
            .ValidatePathSecurity(nameFile, $"Недопустимое имя файла: {nameFile}")
            .ValidateFileExists(_fullPath, $"Не существует файла: {nameFile} в текущей директории");

        try
        {
            File.Delete(_fullPath);
        }
        catch
        {
            throw new ArgumentException($"Ошибка удаления файла");
        }
    }

    public async Task SaveToDatabaseAsync()
    {
        if (_fullPath is null)
        {
            throw new InvalidOperationException("Некорректное поведение системы");
        }

        try
        {
            var fileId = await _fileQueries.GetIdByLocationAsync(_fullPath) ??
                                        throw new DatabaseOperationException($"Файл не найден в базе данных: {_fullPath}", null);

            var operationFile = new OperationFile
            {
                OperationType = OperationTypeFile.Delete,
                ExecutedAt = DateTime.UtcNow,
                FileId = fileId,
                UserId = _menu.UserId,
            };

            await _operationFileRepository.AddAsync(operationFile);
        }
        catch (Exception ex)
        {
            throw new DatabaseOperationException($"Ошибка базы данных при добавлении операции удаления файла '{_fullPath}'", ex);
        }
    }

    private void ResetFiled()
    {
        _fullPath = null;
    }
}