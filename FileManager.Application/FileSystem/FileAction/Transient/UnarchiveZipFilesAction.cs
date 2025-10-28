using FileManager.Application.Extensions;
using FileManager.Application.FileSystem.Interfaces;
using FileManager.Application.Utils;
using System.IO.Compression;

namespace FileManager.Application.FileSystem.FileAction.Transient;

public class UnarchiveZipFilesAction(IMenu menu) : IFileSystemAction
{
    // Лимиты для защиты от ZIP-бомб
    private const long MAX_TOTAL_SIZE = 500 * 1024 * 1024; // 500 MB
    private const long MAX_COMPRESSION_RATIO = 100; // 100:1
    private const int MAX_FILE_COUNT = 10000; // Максимальное количество файлов
    private const long MAX_SINGLE_FILE_SIZE = 50 * 1024 * 1024; // 50 MB на файл

    private const int CountArguments = 2;

    private readonly IMenu _menu = menu ??
        throw new ArgumentNullException(nameof(menu));

    public void Execute(string command)
    {
        var arguments = command.Split(WhitespaceCharsDictionary.AllWhitespace, StringSplitOptions.RemoveEmptyEntries);

        if (arguments.Length != CountArguments)
        {
            throw new ArgumentException($"Некорректное количество аргументов: {command}");
        }

        var nameCommand = arguments.First();
        if (!nameCommand.Equals(CommandDictionary.UnarchiveZip, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException($"Команда: {command} не распознана");
        }

        var nameFileArchive = arguments.Second();
        if (string.IsNullOrWhiteSpace(nameFileArchive))
        {
            throw new ArgumentException("Название файла архива не может быть пустым");
        }

        if (PathSecurity.IsPathTraversal(nameFileArchive))
        {
            throw new ArgumentException($"Недопустимое имя файла архива: {nameFileArchive}");
        }

        var fullPathNameFileArchive = Path.Combine(_menu.Path, nameFileArchive);
        if (!File.Exists(fullPathNameFileArchive))
        {
            throw new ArgumentException($"Не существует архива: {nameFileArchive} в текущей директории");
        }

        ValidateArchiveSafety(fullPathNameFileArchive);

        var nameFileDirectory = Path.GetFileNameWithoutExtension(nameFileArchive);
        var fullPathArchiveDirectory = Path.Combine(_menu.Path, nameFileDirectory);

        if (Directory.Exists(fullPathArchiveDirectory))
        {
            throw new ArgumentException($"Директория для архива: {nameFileArchive} уже существует удалите её или разархивируйте в другой директории");
        }

        Directory.CreateDirectory(fullPathArchiveDirectory);
        SafeExtractArchive(fullPathNameFileArchive, fullPathArchiveDirectory);
    }

    private static void ValidateArchiveSafety(string archivePath)
    {
        using var archive = ZipFile.OpenRead(archivePath);

        long totalCompressedSize = 0;
        long totalUncompressedSize = 0;
        int fileCount = 0;

        foreach (var entry in archive.Entries)
        {
            // Пропускаем записи директорий (они имеют пустое имя или заканчиваются на /)
            if (IsDirectoryEntry(entry))
            {
                continue;
            }

            fileCount++;
            totalCompressedSize += entry.CompressedLength;
            totalUncompressedSize += entry.Length;

            if (fileCount > MAX_FILE_COUNT)
            {
                throw new SecurityException(
                    $"Архив содержит слишком много файлов ({fileCount}). " +
                    $"Максимально допустимо: {MAX_FILE_COUNT}");
            }

            if (entry.Length > MAX_SINGLE_FILE_SIZE)
            {
                throw new SecurityException($"Файл '{entry.FullName}' слишком большой");
            }

            if (entry.CompressedLength > 0)
            {
                var compressionRatio = (double)entry.Length / entry.CompressedLength;
                if (compressionRatio > MAX_COMPRESSION_RATIO)
                {
                    throw new SecurityException("Обнаружен подозрительно высокий коэффициент сжатия");
                }
            }

            if (totalUncompressedSize > MAX_TOTAL_SIZE)
            {
                throw new SecurityException("Общий размер распакованных файлов превышает лимит");
            }
        }

        if (totalCompressedSize > 0)
        {
            var overallCompressionRatio = (double)totalUncompressedSize / totalCompressedSize;
            if (overallCompressionRatio > MAX_COMPRESSION_RATIO)
            {
                throw new SecurityException("Общий коэффициент сжатия архива подозрительно высок");
            }
        }
    }

    private static void SafeExtractArchive(string archivePath, string extractPath)
    {
        using var archive = ZipFile.OpenRead(archivePath);

        long totalExtractedSize = 0;
        int extractedFiles = 0;
        int createdDirectories = 0;

        foreach (var entry in archive.Entries)
        {
            var fullPath = Path.Combine(extractPath, entry.FullName);
            try
            {
                if (IsDirectoryEntry(entry))
                {
                    if (!Directory.Exists(fullPath))
                    {
                        Directory.CreateDirectory(fullPath);
                        createdDirectories++;
                    }
                }
                else
                {
                    var directory = Path.GetDirectoryName(fullPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                        createdDirectories++;
                    }

                    entry.ExtractToFile(fullPath, overwrite: false);
                    totalExtractedSize += entry.Length;
                    extractedFiles++;

                    if (totalExtractedSize > MAX_TOTAL_SIZE)
                    {
                        SafeCleanup(extractPath);
                        throw new SecurityException("Превышен лимит размера при извлечении");
                    }
                }
            }
            catch (Exception)
            {
                throw new InvalidOperationException($"Ошибка при извлечении {entry.FullName}");
            }
        }
    }

    private static bool IsDirectoryEntry(ZipArchiveEntry entry)
    {
        return string.IsNullOrEmpty(entry.Name) ||
               entry.FullName.EndsWith('/') ||
               entry.Length == 0 && entry.FullName.EndsWith('/');
    }

    private static void SafeCleanup(string extractPath)
    {
        try
        {
            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, recursive: true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Предупреждение: не удалось удалить временные файлы: {ex.Message}");
            throw new InvalidOperationException($"Ошибка при извлечении {extractPath}");
        }
    }
}

public class SecurityException : Exception
{
    public SecurityException(string message) : base(message) { }
    public SecurityException(string message, Exception innerException) : base(message, innerException) { }
}