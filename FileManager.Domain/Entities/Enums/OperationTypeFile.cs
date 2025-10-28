namespace FileManager.Domain.Entities.Enums;

/// <summary>
/// Тип операции с файлом
/// </summary>
public enum OperationTypeFile
{
    /// <summary>
    /// Создание файла
    /// </summary>
    Create,

    /// <summary>
    /// Изменение файла
    /// </summary>
    Modify,

    /// <summary>
    /// Удаление файла
    /// </summary>
    Delete
}