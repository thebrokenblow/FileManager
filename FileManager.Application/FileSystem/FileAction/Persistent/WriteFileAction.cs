using FileManager.Application.Extensions;
using FileManager.Application.FileSystem.Interfaces;
using FileManager.Application.Utils;
using FileManager.Domain.Entities;
using FileManager.Domain.Entities.Enums;
using FileManager.Domain.Interfaces.Queries;
using FileManager.Domain.Interfaces.Repositories;

namespace FileManager.Application.FileSystem.FileAction.Persistent;

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

    private string? _nameFile;
    private string? _fullPathFile;

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
        if (!nameCommand.Equals(CommandDictionary.WriteFile, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException($"Команда: {command} не распознана");
        }

        _nameFile = arguments.Second();
        if (string.IsNullOrWhiteSpace(_nameFile))
        {
            throw new ArgumentException("Имя файла не может быть пустым");
        }

        if (PathSecurity.IsPathTraversal(_nameFile))
        {
            throw new ArgumentException($"Недопустимое имя файла: {_nameFile}");
        }

        _fullPathFile = Path.Combine(_menu.Path, _nameFile);
        if (!File.Exists(_fullPathFile))
        {
            throw new ArgumentException($"Файла с именем: {_nameFile} несуществует");
        }

        var sourceText = File.ReadAllText(_fullPathFile);

        _menu.InputText.Invoke(WriteFile, sourceText);
    }

    public async Task SaveToDatabaseAsync()
    {
        if (_fullPathFile is null)
        {
            throw new ArgumentException("Не установлено название файла");
        }

        var fileId = await fileQueries.GetIdByLocation(_fullPathFile) ?? 
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
            throw new ArgumentException("Не установлено название файла");
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