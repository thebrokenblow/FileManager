using FileManager.Application.Extensions;
using FileManager.Application.FileSystem.Interfaces;
using FileManager.Application.Utils;
using FileManager.Domain.Entities;
using FileManager.Domain.Entities.Enums;
using FileManager.Domain.Interfaces.Queries;
using FileManager.Domain.Interfaces.Repositories;

namespace FileManager.Application.FileSystem.FileAction.Persistent;

public class DeleteFileAction(
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

    private string? _fullPath;

    public void Execute(string command)
    {
        if (string.IsNullOrWhiteSpace(command) )
        {
            throw new ArgumentException("Команда не может быть пустой");
        }

        var arguments = command.Split(WhitespaceCharsDictionary.AllWhitespace, StringSplitOptions.RemoveEmptyEntries);
        if (arguments.Length != CountArguments)
        {
            throw new ArgumentException($"Некорректное количество аргументов: {command}");
        }

        var nameCommand = arguments.First();
        if (!nameCommand.Equals(CommandDictionary.DeleteFile, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException($"Команда: {command} не распознана");
        }

        var nameFile = arguments.Second();
        if (string.IsNullOrWhiteSpace(nameFile))
        {
            throw new ArgumentException("Имя файла не может быть пустым");
        }

        if (PathSecurity.IsPathTraversal(nameFile))
        {
            throw new ArgumentException($"Недопустимое имя файла: {nameFile}");
        }

        _fullPath = Path.Combine(menu.Path, nameFile);
        if (!File.Exists(_fullPath))
        {
            throw new ArgumentException($"Не существует файла: {nameFile} в текущей директории");
        }

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
            throw new Exception();
        }

        var fileId = await _fileQueries.GetIdByLocation(_fullPath) ??
                                    throw new Exception("Отсутствует соответствующая запись директории");

        var operationFile = new OperationFile
        {
            OperationType = OperationTypeFile.Delete,
            ExecutedAt = DateTime.UtcNow,
            FileId = fileId,
            UserId = _menu.UserId,
        };

        await _operationFileRepository.AddAsync(operationFile);
    }
}