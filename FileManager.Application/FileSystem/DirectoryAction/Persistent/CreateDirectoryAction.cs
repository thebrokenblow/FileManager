using FileManager.Application.Extensions;
using FileManager.Application.FileSystem.Interfaces;
using FileManager.Application.Utils;
using FileManager.Domain.Entities;
using FileManager.Domain.Interfaces.Repositories;

namespace FileManager.Application.FileSystem.DirectoryAction.Persistent;

public class CreateDirectoryAction(
    IMenu menu, 
    IDirectoryRepository directoryRepository) : IFileSystemAction, IFileSystemPersistentAction
{
    private const int CountArguments = 2;

    private readonly IMenu _menu = menu ?? 
        throw new ArgumentNullException(nameof(menu));

    private readonly IDirectoryRepository _directoryRepository = directoryRepository ?? 
        throw new ArgumentNullException(nameof(directoryRepository));

    private string? _nameDirectory;
    private string? _fullPathDirectory;

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
        if (!nameCommand.Equals(CommandDictionary.CreateDirectory, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException($"Команда: {command} не распознана");
        }

        _nameDirectory = arguments.Second();
        if (string.IsNullOrWhiteSpace(_nameDirectory))
        {
            throw new ArgumentException("Имя директории не может быть пустым");
        }

        if (PathSecurity.IsPathTraversal(_nameDirectory))
        {
            throw new ArgumentException($"Недопустимое имя директории: {_nameDirectory}");
        }

        _fullPathDirectory = Path.Combine(_menu.Path, _nameDirectory);
        if (Directory.Exists(_fullPathDirectory))
        {
            throw new ArgumentException($"Уже существует такая директория: {_nameDirectory}");
        }

        try
        {
            Directory.CreateDirectory(_fullPathDirectory);
        }
        catch
        {
            throw new ArgumentException($"Ошибка создания директории");
        }
    }

    public async Task SaveToDatabaseAsync()
    {
        if (_nameDirectory is null || _fullPathDirectory is null)
        {
            throw new Exception();
        }

        var dateTimeCreateDirectory = DateTime.UtcNow;
        var infoDirectory = new InfoDirectory
        {
            DirectoryName = _nameDirectory,
            CreatedAt = dateTimeCreateDirectory,
            Location = _fullPathDirectory,
            UserId = _menu.UserId
        };

        var operationDirectory = new OperationDirectory
        {
            ExecutedAt = dateTimeCreateDirectory,
            OperationType = Domain.Entities.Enums.OperationTypeDirectory.Create,
            DirectoryId = infoDirectory.Id,
            UserId = _menu.UserId
        };

        await _directoryRepository.AddAsync(infoDirectory, operationDirectory);
    }
}