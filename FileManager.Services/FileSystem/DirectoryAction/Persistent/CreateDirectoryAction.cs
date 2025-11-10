using FileManager.Domain.Interfaces.UseCases;
using FileManager.Domain.Model;
using FileManager.Services.Exceptions;
using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;

namespace FileManager.Services.FileSystem.DirectoryAction.Persistent;

public class CreateDirectoryAction(
    IMenu menu,
    IDirectoryUseCase directoryUseCase) : IFileSystemCommand, IFileSystemPersistent
{
    private const int CountArguments = 2;

    private readonly IMenu _menu = menu ??
        throw new ArgumentNullException(nameof(menu));

    private readonly IDirectoryUseCase _directoryUseCase = directoryUseCase ??
        throw new ArgumentNullException(nameof(directoryUseCase));

    private readonly CommandValidator commandValidator = new();

    private readonly Lock _lockObject = new();

    private string? _nameDirectory;
    private string? _fullPathDirectory;

    public void Execute(string command)
    {
        lock (_lockObject) 
        {
            ResetFiled();

            commandValidator
                .ValidateNotEmpty(command, "Команда не может быть пустой")
                .ValidateArgumentsCount(out string[] arguments, command, CountArguments, $"Некорректное количество аргументов: {command}");

            var nameCommand = arguments.First();
            _nameDirectory = arguments.Second();
            _fullPathDirectory = Path.Combine(_menu.Path, _nameDirectory);

            commandValidator
                .ValidateCommandName(nameCommand, CommandDictionary.CreateDirectory, $"Команда: {command} не распознана")
                .ValidateNotEmpty(_nameDirectory, "Имя директории не может быть пустым")
                .ValidatePathSecurity(_nameDirectory, $"Недопустимое имя директории: {_nameDirectory}")
                .ValidateDirectoryNotExists(_fullPathDirectory, $"Уже существует такая директория: {_nameDirectory}");

            try
            {
                Directory.CreateDirectory(_fullPathDirectory);
            }
            catch
            {
                throw new ArgumentException($"Ошибка создания директории");
            }
        }
    }

    public async Task SaveToDatabaseAsync()
    {
        string nameDirectory;
        string fullPathDirectory;

        if (_nameDirectory is null || _fullPathDirectory is null)
        {
            throw new InvalidOperationException("Некорректное поведение системы");
        }

        nameDirectory = _nameDirectory;
        fullPathDirectory = _fullPathDirectory;

        try
        {
            var dateTimeCreateDirectory = DateTime.UtcNow;

            var directoryModel = new DirectoryModel(
                nameDirectory,
                fullPathDirectory,
                dateTimeCreateDirectory);

            await _directoryUseCase.CreateAsync(directoryModel, _menu.UserId);
        }
        catch (Exception ex)
        {
            throw new DatabaseOperationException($"Ошибка базы данных при создании директории '{nameDirectory}'", ex);
        }
    }

    private void ResetFiled()
    {
        _nameDirectory = null;
        _fullPathDirectory = null;
    }
}