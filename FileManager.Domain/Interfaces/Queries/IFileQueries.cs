using FileManager.Domain.Entities;

namespace FileManager.Domain.Interfaces.Queries;

/// <summary>
/// Интерфейс для выполнения запросов к файлам
/// </summary>
public interface IFileQueries
{
    /// <summary>
    /// Получить идентификатор файла по полному пути
    /// </summary>
    /// <param name="fullPath">Полный путь к файлу</param>
    /// <returns>Идентификатор файла или null, если файл не найден</returns>
    Task<int?> GetIdByLocationAsync(string fullPath);

    /// <summary>
    /// Получить информацию о файле по полному пути
    /// </summary>
    /// <param name="fullPath">Полный путь к файлу</param>
    /// <returns>Информация о файле или null, если файл не найден</returns>
    Task<InfoFile?> GetByLocationAsync(string fullPath);
}