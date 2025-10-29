using System.Xml;
using System.Xml.Linq;

namespace FileManager.Services.Parsers;

public class XmlParser
{
    public static void ReadXmlFile(string filePath)
    {
        var xmlReaderSettings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersFromEntities = 1024,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            IgnoreWhitespace = true
        };

        using var fileStream = File.OpenRead(filePath);
        using var xmlReader = XmlReader.Create(fileStream, xmlReaderSettings);

        var xmlDocument = XDocument.Load(xmlReader) ??
                          throw new ArgumentException("Ошибка десериализации");

        if (xmlDocument.Root is null)
        {
            throw new ArgumentException("Ошибка десериализации");
        }

        PrintExactFormat(xmlDocument.Root, 0);
    }

    private static void PrintExactFormat(XElement element, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 2);

        // Обрабатываем корневой элемент
        if (indentLevel == 0)
        {
            Console.WriteLine($"{indent}{element.Name}:");
            foreach (var child in element.Elements())
            {
                PrintElement(child, indentLevel + 1);
            }
            return;
        }

        PrintElement(element, indentLevel);
    }

    private static void PrintElement(XElement element, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 2);
        var parent = element.Parent;

        // Проверяем, есть ли у родителя несколько дочерних элементов с таким же именем
        var sameNameSiblings = parent?.Elements(element.Name).ToList() ?? new List<XElement>();
        bool isMultiple = sameNameSiblings.Count > 1;

        if (isMultiple && element == sameNameSiblings.First())
        {
            // Выводим заголовок для группы одинаковых элементов
            Console.WriteLine($"{indent}{element.Name}:");

            foreach (var sibling in sameNameSiblings)
            {
                Console.Write($"{indent}  ");
                PrintElementContent(sibling, indentLevel + 1);
            }
        }
        else if (!isMultiple)
        {
            // Одиночный элемент
            Console.Write($"{indent}{element.Name}:");
            PrintElementContent(element, indentLevel);
        }
    }

    private static void PrintElementContent(XElement element, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 2);

        // Получаем текстовое содержимое
        var textNodes = element.Nodes()
                              .Where(n => n.NodeType == XmlNodeType.Text)
                              .Select(n => n.ToString().Trim())
                              .Where(s => !string.IsNullOrEmpty(s))
                              .ToList();

        // Получаем атрибуты
        var attributes = element.Attributes().ToList();

        // Получаем дочерние элементы
        var childElements = element.Elements().ToList();

        // ВЫВОДИМ АТРИБУТЫ ДАЖЕ ЕСЛИ ЕСТЬ ДОЧЕРНИЕ ЭЛЕМЕНТЫ
        if (attributes.Any())
        {
            Console.Write($" {string.Join(", ", attributes.Select(a => $"{a.Name}: {SecurityEncode(a.Value)}"))}");
        }

        // Если есть только текст и нет дочерних элементов
        if (textNodes.Any() && !childElements.Any())
        {
            string text = string.Join(" ", textNodes);
            if (attributes.Any())
            {
                Console.WriteLine($", value: {SecurityEncode(text)}");
            }
            else
            {
                Console.WriteLine($" {SecurityEncode(text)}");
            }
        }
        // Если есть дочерние элементы
        else if (childElements.Any())
        {
            // Если были атрибуты, переходим на новую строку
            if (attributes.Any())
            {
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine();
            }

            foreach (var child in childElements)
            {
                PrintElement(child, indentLevel + 1);
            }
        }
        // Если есть только атрибуты
        else if (attributes.Any() && !textNodes.Any() && !childElements.Any())
        {
            Console.WriteLine();
        }
        // Пустой элемент
        else
        {
            Console.WriteLine();
        }
    }

    private static string SecurityEncode(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return input.Replace("\r", "\\r")
                    .Replace("\n", "\\n")
                    .Replace("\t", "\\t");
    }
}