using FileManager.Domain.Entities;
using FileManager.Domain.Entities.Enums;
using FileManager.Domain.Interfaces.Queries;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;

namespace FileManager.Services.FileSystem.FileAction.Persistent;

public class WriteFileAction(
    IMenu menu,
    IFileQueries fileQueries,
    IOperationFileRepository operationFileRepository) : IFileSystemAction, IFileSystemPersistentAction
{
    private const int CountArguments = 2;

    private readonly IMenu _menu = menu ??
        throw new ArgumentNullException(nameof(menu));

    private readonly IFileQueries _fileQueries = fileQueries ??
        throw new ArgumentNullException(nameof(fileQueries));

    private readonly IOperationFileRepository _operationFileRepository = operationFileRepository ??
        throw new ArgumentNullException(nameof(operationFileRepository));

    private readonly CommandValidator commandValidator = new();
    private string? _nameFile;
    private string? _fullPathFile;

    public void Execute(string command)
    {
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
        if (_fullPathFile is null)
        {
            throw new InvalidOperationException("Некорректное поведение системы");
        }

        var fileId = await _fileQueries.GetIdByLocation(_fullPathFile) ?? 
                                throw new Exception();

        var operationFile = new OperationFile
        {
            OperationType = OperationTypeFile.Modify,
            ExecutedAt = DateTime.UtcNow,
            FileId = fileId,
            UserId = _menu.UserId,
        };

        await _operationFileRepository.AddAsync(operationFile);
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
        }
        catch
        {
            throw new ArgumentException("Ошибка записи в файла");
        }
    }
}