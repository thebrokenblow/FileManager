using FileManager.Application.Actions.Interfaces;
using FileManager.Application.Extensions;
using FileManager.Application.Utils;
using FileManager.Data.Repositories;
using FileManager.Extensions;

namespace FileManager.Application.Actions.FileAction;

public class CopyFileAction(
    Menu menu, 
    DirectoryPath directoryPath, 
    FileRepository fileRepository) : IAction
{
    private const int CountArguments = 3;
    private const string ArgumentMoveFileBelow = "-l";

    public void Execute(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            throw new ArgumentException("Команда не может быть пустой");
        }

        var arguments = command.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        if (arguments.Length != CountArguments)
        {
            throw new ArgumentException($"Некорректное количество аргументов: {command}");
        }

        var nameCommand = arguments.First();
        if (!nameCommand.Equals(CommandDictionary.CopyFile, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException($"Команда: {command} не распознана");
        }

        var nameSourceFile = arguments.Second();
        if (string.IsNullOrWhiteSpace(nameSourceFile))
        {
            throw new ArgumentException("Название файла не может быть пустым");
        }

        if (PathSecurity.IsPathTraversal(nameSourceFile))
        {
            throw new ArgumentException($"Недопустимое имя файла: {nameSourceFile}");
        }

        var fullPathSourceFile = Path.Combine(menu.CurrentPath, nameSourceFile);
        if (!File.Exists(fullPathSourceFile))
        {
            throw new ArgumentException($"Не существует файла с названием: {nameSourceFile}");
        }

        var nameDestinationDirectory = arguments.Third();
        if (nameDestinationDirectory.Equals(ArgumentMoveFileBelow, StringComparison.CurrentCultureIgnoreCase))
        {
            CopyFileToDirectoryBelow(nameSourceFile, fullPathSourceFile);
            return;
        }

        if (string.IsNullOrWhiteSpace(nameDestinationDirectory))
        {
            throw new ArgumentException("Название директории не должно быть пустым");
        }

        if (PathSecurity.IsPathTraversal(nameDestinationDirectory))
        {
            throw new ArgumentException($"Недопустимое имя директории: {nameDestinationDirectory}");
        }

        var fullPathDestinationDirectory = Path.Combine(menu.CurrentPath, nameDestinationDirectory);
        if (!Directory.Exists(fullPathDestinationDirectory))
        {
            throw new ArgumentException($"Не существует директории с названием: {nameDestinationDirectory}");
        }

        var fullPathDestinationFile = Path.Combine(fullPathDestinationDirectory, nameSourceFile);
        if (Directory.Exists(fullPathDestinationFile))
        {
            throw new ArgumentException($"Файл с названием: {nameSourceFile} уже существует в директории: {nameDestinationDirectory}");
        }

        CopyFile(fullPathSourceFile, fullPathDestinationFile);
        fileRepository.Create(menu.UserId, nameSourceFile, fullPathDestinationFile);
    }

    private void CopyFileToDirectoryBelow(string nameSourceFile, string fullPathSourceFile)
    {
        var directoryBelow = directoryPath.GetDirectoryBelow();
        var fullPathFileBelow = Path.Combine(directoryBelow, nameSourceFile);

        if (File.Exists(fullPathFileBelow))
        {
            throw new ArgumentException($"Уже существует файл: {nameSourceFile} на уровне ниже");
        }

        CopyFile(fullPathSourceFile, fullPathFileBelow);
    }

    private static void CopyFile(string sourceFileName, string destFileName)
    {
        try
        {
            File.Copy(sourceFileName, destFileName);
        }
        catch
        {
            throw new ArgumentException($"Ошибка перемещения деректории");
        }
    }
}
