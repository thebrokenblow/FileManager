using FileManager.Domain.Entities.Enums;

namespace FileManager.Domain.Entities;

/// <summary>
/// Сущность операции с директорией
/// </summary>
public class OperationDirectory
{
    /// <summary>
    /// Уникальный идентификатор операции
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Дата и время выполнения операции
    /// </summary>
    public required DateTime ExecutedAt { get; set; }

    /// <summary>
    /// Тип выполненной операции
    /// </summary>
    public required OperationTypeDirectory OperationType { get; set; }

    /// <summary>
    /// Идентификатор директории, с которым выполнена операция
    /// </summary>
    public required int DirectoryId { get; set; }

    /// <summary>
    /// Навигационное свойство для директории, с которым выполнена операция
    /// </summary>
    public InfoDirectory? Directory { get; set; }

    /// <summary>
    /// Идентификатор пользователя, выполнившего операцию
    /// </summary>
    public required int UserId { get; set; }

    /// <summary>
    /// Навигационное свойство для пользователя, выполнившего операцию
    /// </summary>
    public User? User { get; set; }
}