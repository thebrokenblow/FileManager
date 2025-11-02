using FileManager.Domain.Interfaces.UseCases;
using FileManager.Domain.Model;
using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;

namespace FileManager.Services.FileSystem.DirectoryAction.Persistent;

public class DeleteDirectoryAction(
    IMenu menu,
    IDirectoryUseCase directoryUseCase) : IFileSystemAction, IFileSystemPersistentAction
{
    private const int CountArguments = 2;

    private readonly IMenu _menu = menu ??
        throw new ArgumentNullException(nameof(menu));

    private readonly IDirectoryUseCase _directoryUseCase = directoryUseCase ??
        throw new ArgumentNullException(nameof(directoryUseCase));

    private readonly CommandValidator commandValidator = new();

    private string? _fullPathDirectory;
    private string[]? childLocationsFiles;
    private string[]? childLocationsDirectories;

    public void Execute(string command)
    {
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


        childLocationsFiles = Directory.GetFiles(_fullPathDirectory, "*", SearchOption.AllDirectories);
        childLocationsDirectories = Directory.GetDirectories(_fullPathDirectory, "*", SearchOption.AllDirectories);

        try
        {
            Directory.Delete(_fullPathDirectory, true);
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

        var deleteDirectoryModel = new DeleteDirectoryModel(
            DateTime.UtcNow,
            _fullPathDirectory,
            _menu.UserId,
            childLocationsFiles,
            childLocationsDirectories);

        await _directoryUseCase.DeleteDirectoryAsync(deleteDirectoryModel);
    }
}