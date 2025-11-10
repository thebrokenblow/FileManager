using FileManager.Domain.Interfaces.UseCases;
using FileManager.Domain.Model;
using FileManager.Services.Exceptions;
using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;

namespace FileManager.Services.FileSystem.DirectoryAction.Persistent;

public class DeleteDirectoryAction(
    IMenu menu,
    IDirectoryUseCase directoryUseCase) : IFileSystemCommand, IFileSystemPersistent
{
    private const int CountArguments = 2;

    private readonly IMenu _menu = menu ??
        throw new ArgumentNullException(nameof(menu));

    private readonly IDirectoryUseCase _directoryUseCase = directoryUseCase ??
        throw new ArgumentNullException(nameof(directoryUseCase));

    private readonly CommandValidator commandValidator = new();

    private string? _fullPathDirectory;
    private string[]? _childLocationsFiles;
    private string[]? _childLocationsDirectories;

    public void Execute(string command)
    {
        ResetFiled();

        commandValidator
            .ValidateNotEmpty(command, "Команда не может быть пустой")
            .ValidateArgumentsCount(out string[] arguments, command, CountArguments, $"Некорректное количество аргументов: {command}");

        var nameCommand = arguments.First();
        var nameDirectory = arguments.Second();
        _fullPathDirectory = Path.Combine(_menu.Path, nameDirectory);

        commandValidator
            .ValidateCommandName(nameCommand, CommandDictionary.DeleteDirectory, $"Команда: {command} не распознана")
            .ValidateNotEmpty(nameDirectory, "Имя директории не может быть пустым")
            .ValidatePathSecurity(nameDirectory, $"Недопустимое имя директории: {nameDirectory}")
            .ValidateDirectoryExists(_fullPathDirectory, $"Нет дериктории с именем: {nameDirectory}");


        _childLocationsFiles = Directory.GetFiles(_fullPathDirectory, "*", SearchOption.AllDirectories);
        _childLocationsDirectories = Directory.GetDirectories(_fullPathDirectory, "*", SearchOption.AllDirectories);

        try
        {
            Directory.Delete(_fullPathDirectory, recursive: true);
        }
        catch
        {
            throw new ArgumentException($"Ошибка удаления файла");
        }
    }

    public async Task SaveToDatabaseAsync()
    {
        if (_fullPathDirectory is null)
        {
            throw new InvalidOperationException("Некорректное поведение системы");
        }

        try
        {
            var dateTimeDirectoryDelete = DateTime.UtcNow;

            var deleteDirectoryModel = new DeleteDirectoryModel(
                dateTimeDirectoryDelete,
                _fullPathDirectory,
                _childLocationsFiles,
                _childLocationsDirectories);

            await _directoryUseCase.DeleteAsync(deleteDirectoryModel, _menu.UserId);
        }
        catch (Exception ex)
        {
            throw new DatabaseOperationException($"Ошибка базы данных при удалении директории '{_fullPathDirectory}'", ex);
        }
    }

    private void ResetFiled()
    {
        _fullPathDirectory = null;
        _childLocationsFiles = null;
        _childLocationsDirectories = null;
    }
}