using FileManager.Domain.Entities;

namespace FileManager.Domain.Interfaces.Queries;

/// <summary>
/// Интерфейс для выполнения запросов к директориям
/// </summary>
public interface IDirectoryQueries
{
    /// <summary>
    /// Получить информацию о директории по полному пути
    /// </summary>
    /// <param name="fullPath">Полный путь к директории</param>
    /// <returns>Информация о директории или null, если директория не найдена</returns>
    Task<InfoDirectory?> GetByLocationAsync(string fullPath);

    /// <summary>
    /// Получить идентификатор директории по полному пути
    /// </summary>
    /// <param name="fullPath">Полный путь к директории</param>
    /// <returns>Идентификатор директории или null, если директория не найдена</returns>
    Task<int?> GetIdByLocationAsync(string fullPath);
}