using FileManager.Application.Actions.Interfaces;
using FileManager.Application.Extensions;
using FileManager.Application.Utils;
using FileManager.Data;
using FileManager.Extensions;
using FileManager.Parsers;
using System.Xml;

namespace FileManager.Application.Actions.FileAction;

public class DeserializeXmlFileAction(Menu menu, FileManagerContext context) : IAction
{
    private const int CountArguments = 2;

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

        var fullPathFile = Path.Combine(menu.CurrentPath, nameFile);
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