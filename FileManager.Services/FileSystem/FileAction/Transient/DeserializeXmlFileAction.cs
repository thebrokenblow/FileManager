using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Parsers;
using FileManager.Services.Utils;
using FileManager.Services.Validation;
using System.Xml;

namespace FileManager.Services.FileSystem.FileAction.Transient;

public class DeserializeXmlFileAction(IMenu menu) : IFileSystemAction
{
    private const int CountArguments = 2;

    private readonly IMenu _menu = menu ??
        throw new ArgumentNullException(nameof(menu));

    private readonly CommandValidator commandValidator = new();

    public void Execute(string command)
    {
        commandValidator
           .ValidateNotEmpty(command, "Команда не может быть пустой")
           .ValidateArgumentsCount(out string[] arguments, command, CountArguments, $"Некорректное количество аргументов: {command}");

        var nameCommand = arguments.First();
        var nameFile = arguments.Second();
        var fullPathFile = Path.Combine(_menu.Path, nameFile);

        commandValidator
            .ValidateCommandName(nameCommand, CommandDictionary.DeserializeXml, $"Команда: {command} не распознана")
            .ValidateNotEmpty(nameFile, "Имя файла не может быть пустым")
            .ValidatePathSecurity(nameFile, $"Недопустимое имя файла: {nameFile}")
            .ValidateFileExists(fullPathFile, $"Файла с именем: {nameFile} несуществует");

        try
        {
            XmlParser.ReadXmlFile(fullPathFile);
        }
        catch (XmlException ex) when (ex.Message.Contains("DTD") || ex.Message.Contains("entity"))
        {
            throw new Exception("XML содержит запрещенные DTD или декларации сущностей");
        }
        catch
        {
            throw new Exception("Ошибка десериализации");
        }
    }
}