namespace FileManager.Domain.Entities;

/// <summary>
/// Сущность файла
/// </summary>
public class InfoFile
{
    /// <summary>
    /// Уникальный идентификатор файла
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Имя файла
    /// </summary>
    public required string Filename { get; set; }

    /// <summary>
    /// Дата и время создания файла
    /// </summary>
    public required DateTime CreatedAt { get; set; }

    /// <summary>
    /// Размер файла в байтах
    /// </summary>
    public required long Size { get; set; }

    /// <summary>
    /// Расположение файла
    /// </summary>
    public required string Location { get; set; }

    /// <summary>
    /// Идентификатор владельца файла
    /// </summary>
    public required int UserId { get; set; }

    /// <summary>
    /// Навигационное свойство для владельца файла
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Навигационное свойство для операций, выполненных с файлом
    /// </summary>
    public List<OperationFile>? Operations { get; set; }
}