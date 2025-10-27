using FileManager.Data.Entities.Enums;

namespace FileManager.Data.Entities;

/// <summary>
/// Сущность операции с файлом
/// </summary>
public class OperationFile
{
    /// <summary>
    /// Уникальный идентификатор операции
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Дата и время выполнения операции
    /// </summary>
    public required DateTime Timestamp { get; set; }

    /// <summary>
    /// Тип выполненной операции
    /// </summary>
    public required OperationTypeFile OperationType { get; set; }

    /// <summary>
    /// Идентификатор файла, с которым выполнена операция
    /// </summary>
    public required int FileId { get; set; }

    /// <summary>
    /// Навигационное свойство для файла, с которым выполнена операция
    /// </summary>
    public InfoFile? File { get; set; }

    /// <summary>
    /// Идентификатор пользователя, выполнившего операцию
    /// </summary>
    public required int UserId { get; set; }

    /// <summary>
    /// Навигационное свойство для пользователя, выполнившего операцию
    /// </summary>
    public User? User { get; set; }
}