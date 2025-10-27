using FileManager.Application.Actions.Interfaces;
using FileManager.Application.Extensions;
using FileManager.Application.Utils;
using FileManager.Data.Repositories;
using FileManager.Extensions;

namespace FileManager.Application.Actions.FileAction;

public class MoveFileAction(
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
        if (!nameCommand.Equals(CommandDictionary.MoveFile, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException($"Команда: {command} не распознана");
        }

        var nameSourceFile = arguments.Second();
        if (string.IsNullOrWhiteSpace(nameSourceFile))
        {
            throw new ArgumentException("Название файла не может быть пустым");
        }

        var fullPathSourceFile = Path.Combine(menu.CurrentPath, nameSourceFile);
        if (!File.Exists(fullPathSourceFile))
        {
            throw new ArgumentException($"Не существует файла с названием: {nameSourceFile}");
        }

        if (PathSecurity.IsPathTraversal(nameSourceFile))
        {
            throw new ArgumentException($"Недопустимое имя файла: {nameSourceFile}");
        }

        var nameDestinationDirectory = arguments.Third();
        if (nameDestinationDirectory.Equals(ArgumentMoveFileBelow, StringComparison.CurrentCultureIgnoreCase))
        {
            MoveFileToDirectoryBelow(nameSourceFile, fullPathSourceFile);
            return;
        }

        if (string.IsNullOrWhiteSpace(nameDestinationDirectory))
        {
            throw new ArgumentException("Название директории не может быть пустым");
        }

        var fullPathDestinationDirectory = Path.Combine(menu.CurrentPath, nameDestinationDirectory);
        if (!Directory.Exists(fullPathDestinationDirectory))
        {
            throw new ArgumentException($"Не существует директории с названием: {nameDestinationDirectory}");
        }

        var fullNameDestinationDirectoryFile = Path.Combine(fullPathDestinationDirectory, nameSourceFile);

        MoveFile(fullPathSourceFile, fullNameDestinationDirectoryFile);

        var infoFile = fileRepository.GetByLocation(fullPathSourceFile);
        infoFile!.Location = fullNameDestinationDirectoryFile;
        fileRepository.Update(infoFile);
    }

    private void MoveFileToDirectoryBelow(string nameSourceFile, string fullPathSourceFile)
    {
        var directoryBelow = directoryPath.GetDirectoryBelow();
        string fullPathFileBelow = Path.Combine(directoryBelow, nameSourceFile);

        if (File.Exists(fullPathFileBelow))
        {
            throw new ArgumentException($"Уже существует файл: {nameSourceFile} на уровне ниже");
        }

        MoveFile(fullPathSourceFile, fullPathFileBelow);

        var infoFile = fileRepository.GetByLocation(fullPathSourceFile);
        infoFile!.Location = fullPathFileBelow;
        fileRepository.Update(infoFile);
    }

    private static void MoveFile(string sourceFileName, string destFileName)
    {
        try
        {
            File.Move(sourceFileName, destFileName);
        }
        catch
        {
            throw new ArgumentException($"Ошибка перемещения файла");
        }
    }
}