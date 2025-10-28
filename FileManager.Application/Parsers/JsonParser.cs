using System.Text.Json;
using System.Text.Json.Serialization;

namespace FileManager.Application.Parsers;

public static class JsonParser
{
    private static readonly JsonSerializerOptions SafeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.Strict,
        MaxDepth = 32,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static void ReadJsonFile(string filePath)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > 10 * 1024 * 1024)
            {
                throw new Exception("Файл слишком большой");
            }

            string jsonContent = File.ReadAllText(filePath);

            using JsonDocument doc = JsonDocument.Parse(jsonContent, new JsonDocumentOptions
            {
                MaxDepth = 32,
                AllowTrailingCommas = false
            });

            PrintJsonStructure(doc.RootElement, 0);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Ошибка парсинга JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Ошибка чтения файла: {ex.Message}");
        }
    }

    private static void PrintJsonStructure(JsonElement element, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 2);

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    Console.Write($"{indent}{property.Name}: ");
                    if (property.Value.ValueKind == JsonValueKind.Object ||
                        property.Value.ValueKind == JsonValueKind.Array)
                    {
                        Console.WriteLine();
                    }
                    PrintJsonValue(property.Value, indentLevel + 1);
                }
                break;

            case JsonValueKind.Array:
                Console.WriteLine($"{indent}[");
                foreach (var item in element.EnumerateArray())
                {
                    PrintJsonStructure(item, indentLevel + 1);
                }
                Console.WriteLine($"{indent}]");
                break;

            default:
                PrintJsonValue(element, indentLevel);
                break;
        }
    }

    private static void PrintJsonValue(JsonElement element, int indentLevel)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                Console.WriteLine($"{element.GetString()}");
                break;
            case JsonValueKind.Number:
                Console.WriteLine($"{element.GetRawText()}");
                break;
            case JsonValueKind.True:
                Console.WriteLine($"true");
                break;
            case JsonValueKind.False:
                Console.WriteLine($"false");
                break;
            case JsonValueKind.Null:
                Console.WriteLine($"null");
                break;
            default:
                PrintJsonStructure(element, indentLevel);
                break;
        }
    }

    public static T DeserializeSafe<T>(string jsonContent, JsonSerializerOptions? customOptions = null)
    {
        var options = customOptions ?? SafeOptions;
        options.MaxDepth = Math.Min(options.MaxDepth, 64);

        return JsonSerializer.Deserialize<T>(jsonContent, options)
            ?? throw new ArgumentException("Ошибка десериализации: результат null");
    }
}