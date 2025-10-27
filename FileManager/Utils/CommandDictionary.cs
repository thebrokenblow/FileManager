namespace FileManager.Utils;

/// <summary>
/// Словарь поддерживаемых команд для операций с файлами и директориями.
/// Содержит константы для распространенных команд в терминологии аналогичной Unix/Linux.
/// </summary>
public class CommandDictionary
{
    // Операции с директориями

    /// <summary>
    /// Команда для изменения текущей рабочей директории
    /// </summary>
    public const string ChangeDirectory = "cd";

    /// <summary>
    /// Команда для перехода в родительскую директорию
    /// </summary>
    public const string MoveToParentDirectory = "cd..";

    /// <summary>
    /// Команда для создания новой директории
    /// </summary>
    public const string CreateDirectory = "mkdir";

    /// <summary>
    /// Команда для удаления директории
    /// </summary>
    public const string DeleteDirectory = "rmdir";

    /// <summary>
    /// Команда для перемещения
    /// </summary>
    public const string MoveDirectory = "mvd";

    /// <summary>
    /// Команда для отображения содержимого директории (файлов и поддиректорий)
    /// </summary>
    public const string ShowDirectoriesAndFiles = "ls";

    // Операции с файлами

    /// <summary>
    /// Команда для копирования файла
    /// </summary>
    public const string CopyFile = "cp";

    /// <summary>
    /// Команда для создания нового файла
    /// </summary>
    public const string CreateFile = "touch";

    /// <summary>
    /// Команда для удаления файла
    /// </summary>
    public const string DeleteFile = "rm";

    /// <summary>
    /// Команда для чтения и вывода содержимого файла
    /// </summary>
    public const string ReadFile = "cat";

    /// <summary>
    /// Команда для записи текста в файл
    /// </summary>
    public const string WriteFile = "echo";

    /// <summary>
    /// Команда для перемещения
    /// </summary>
    public const string MoveFile = "mvf";

    /// <summary>
    /// Команда для десериализации XML файла
    /// </summary>
    public const string DeserializeXml = "parsexml";

    /// <summary>
    /// Команда для десериализации JSON файла
    /// </summary>
    public const string DeserializeJson = "parsejson";

    /// <summary>
    /// Команда для aрхивирования файлов
    /// </summary>
    public const string ArchiveZip = "zip+";

    /// <summary>
    /// Команда для разархивирования файлов
    /// </summary>
    public const string UnarchiveZip = "zip-";

    /// <summary>
    /// Команда для отображения информации о дисковых накопителях (доступное место, размер)
    /// </summary>
    public const string ShowDiskInfo = "df";

    /// <summary>
    /// Команда для отображения справочной информации
    /// </summary>
    public const string Help = "help";
}