namespace FileManager.Data.Entities;

/// <summary>
/// Сущность директории
/// </summary>
public class InfoDirectory
{
    /// <summary>
    /// Уникальный идентификатор директории
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Имя директории
    /// </summary>
    public required string DirectoryName { get; set; }

    /// <summary>
    /// Дата и время создания директории
    /// </summary>
    public required DateTime CreatedAt { get; set; }

    /// <summary>
    /// Расположение директории
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Идентификатор владельца директории
    /// </summary>
    public required int UserId { get; set; }

    /// <summary>
    /// Навигационное свойство для владельца директории
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Навигационное свойство для операций, выполненных с директориями
    /// </summary>
    public List<OperationDirectory>? Operations { get; set; }
}