using FileManager.Domain.Entities;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Domain.Model;
using FileManager.Services.Exceptions;
using FileManager.Services.Extensions;
using FileManager.Services.FileSystem.DirectoryAction.Persistent;
using FileManager.Services.FileSystem.Interfaces;
using FileManager.Services.Utils;
using FileManager.Services.Validation;
using System.IO.Compression;

namespace FileManager.Services.FileSystem.FileAction.Persistent;

public class UnarchiveZipFilesAction(
    IMenu menu,
    IFileRepository fileRepository,
    CreateDirectoryAction createDirectoryAction) : IFileSystemAction, IFileSystemPersistentAction
{
    // Лимиты для защиты от ZIP-бомб
    // 500 MB
    private const long MaxTotalSize = 500 * 1024 * 1024;

    // 100:1
    private const long MaxComperssionSize = 100;

    // Максимальное количество файлов
    private const int MaxFileCount = 10000;

    // 50 MB на файл
    private const long MaxSingleFileSize = 50 * 1024 * 1024; 
    
    private const int CountArguments = 2;

    private readonly IMenu _menu = menu ??
        throw new ArgumentNullException(nameof(menu));

    private readonly IFileRepository _fileRepository = fileRepository ??
        throw new ArgumentNullException(nameof(fileRepository));

    private readonly CreateDirectoryAction _createDirectoryAction = createDirectoryAction ??
        throw new ArgumentNullException(nameof(createDirectoryAction));

    private readonly CommandValidator commandValidator = new();
    private List<FileModel>? _files;

    public void Execute(string command)
    {
        ResetFiled();

        commandValidator
          .ValidateNotEmpty(command, "Команда не может быть пустой")
          .ValidateArgumentsCount(out string[] arguments, command, CountArguments, $"Некорректное количество аргументов: {command}");

        var nameCommand = arguments.First();
        var nameFileArchive = arguments.Second();
        var fullPathNameFileArchive = Path.Combine(_menu.Path, nameFileArchive);

        commandValidator
            .ValidateCommandName(nameCommand, CommandDictionary.UnarchiveZip, $"Команда: {command} не распознана")
            .ValidateNotEmpty(nameFileArchive, "Название файла архива не может быть пустым")
            .ValidatePathSecurity(nameFileArchive, $"Недопустимое имя файла архива: {nameFileArchive}")
            .ValidateFileExists(fullPathNameFileArchive, $"Не существует архива: {nameFileArchive} в текущей директории");

        ValidateArchiveSafety(fullPathNameFileArchive);

        var nameFileDirectory = Path.GetFileNameWithoutExtension(nameFileArchive);
        var fullPathArchiveDirectory = Path.Combine(_menu.Path, nameFileDirectory);

        commandValidator
            .ValidateDirectoryNotExists(
                fullPathArchiveDirectory,
                $"Директория для архива: {nameFileArchive} уже существует удалите её или разархивируйте в другой директории");

        try
        {
            _createDirectoryAction.Execute($"mkdir {nameFileDirectory}");
        }
        catch
        {
            throw;
        }

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

            if (fileCount > MaxFileCount)
            {
                throw new SecurityException(
                    $"Архив содержит слишком много файлов ({fileCount}). " +
                    $"Максимально допустимо: {MaxFileCount}");
            }

            if (entry.Length > MaxSingleFileSize)
            {
                throw new SecurityException($"Файл '{entry.FullName}' слишком большой");
            }

            if (entry.CompressedLength > 0)
            {
                var compressionRatio = (double)entry.Length / entry.CompressedLength;
                if (compressionRatio > MaxComperssionSize)
                {
                    throw new SecurityException("Обнаружен подозрительно высокий коэффициент сжатия");
                }
            }

            if (totalUncompressedSize > MaxTotalSize)
            {
                throw new SecurityException("Общий размер распакованных файлов превышает лимит");
            }
        }

        if (totalCompressedSize > 0)
        {
            var overallCompressionRatio = (double)totalUncompressedSize / totalCompressedSize;
            if (overallCompressionRatio > MaxComperssionSize)
            {
                throw new SecurityException("Общий коэффициент сжатия архива подозрительно высок");
            }
        }
    }

    private void SafeExtractArchive(string archivePath, string extractPath)
    {
        using var archive = ZipFile.OpenRead(archivePath);

        long totalExtractedSize = 0;
        int extractedFiles = 0;
        int createdDirectories = 0;

        _files = [];

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

                    var createAt = DateTime.UtcNow;

                    var fileModel = new FileModel(
                        entry.FullName,
                        fullPath,
                        createAt,
                        entry.Length);

                    _files.Add(fileModel);

                    entry.ExtractToFile(fullPath, overwrite: false);
                    totalExtractedSize += entry.Length;
                    extractedFiles++;

                    if (totalExtractedSize > MaxTotalSize)
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

    public async Task SaveToDatabaseAsync()
    {
        if (_files is null)
        {
            return;
        }

        try
        {
            var infoFiles = _files.Select(file => new InfoFile
            {
                Filename = file.FullPath,
                Size = file.Size,
                CreatedAt = file.CreateAt,
                Location = file.FullPath,
                UserId = _menu.UserId,
            })
            .ToList();

            await _fileRepository.AddAsync(infoFiles);
        }
        catch (Exception ex)
        {
            throw new DatabaseOperationException($"Ошибка базы данных при добавлении {_files?.Count} файлов", ex);
        }
    }

    private void ResetFiled()
    {
        _files = null;
    }
}