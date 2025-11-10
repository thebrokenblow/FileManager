using FileManager.Domain.Entities;

namespace FileManager.Domain.Interfaces.Repositories;

/// <summary>
/// Интерфейс репозитория для работы с директориями
/// </summary>
public interface IDirectoryRepository
{
    /// <summary>
    /// Добавить новую директорию
    /// </summary>
    /// <param name="infoDirectory">Информация о директории для добавления</param>
    /// <returns>Задача, представляющая асинхронную операцию</returns>
    Task AddAsync(InfoDirectory infoDirectory);

    /// <summary>
    /// Обновить информацию о существующей директории
    /// </summary>
    /// <param name="infoDirectory">Информация о директории для обновления</param>
    /// <returns>Задача, представляющая асинхронную операцию</returns>
    Task UpdateAsync(InfoDirectory infoDirectory);
}