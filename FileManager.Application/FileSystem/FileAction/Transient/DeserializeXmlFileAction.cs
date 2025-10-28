using FileManager.Application.Extensions;
using FileManager.Application.FileSystem.Interfaces;
using FileManager.Application.Parsers;
using FileManager.Application.Utils;
using System.Xml;

namespace FileManager.Application.FileSystem.FileAction.Transient;

public class DeserializeXmlFileAction(IMenu menu) : IFileSystemAction
{
    private const int CountArguments = 2;

    private readonly IMenu _menu = menu ??
        throw new ArgumentNullException(nameof(menu));

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
        if (!nameCommand.Equals(CommandDictionary.DeserializeXml, StringComparison.CurrentCultureIgnoreCase))
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

        var fullPathFile = Path.Combine(_menu.Path, nameFile);
        if (!File.Exists(fullPathFile))
        {
            throw new ArgumentException($"Файла с именем: {nameFile} несуществует");
        }

        try
        {
            XmlParser.ReadXmlFile(fullPathFile);
        }
        catch (XmlException ex) when (ex.Message.Contains("DTD") || ex.Message.Contains("entity"))
        {
            throw new Exception("XML содержит запрещенные DTD или декларации сущностей");
        }
    }
}