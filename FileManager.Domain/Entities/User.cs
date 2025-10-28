namespace FileManager.Domain.Entities;

/// <summary>
/// Сущность пользователя
/// </summary>
public class User
{
    /// <summary>
    /// Уникальный идентификатор пользователя
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Хэш пароля пользователя
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// Навигационное свойство для файлов, принадлежащих пользователю
    /// </summary>
    public List<InfoFile>? Files { get; set; }

    /// <summary>
    /// Навигационное свойство для директорий, принадлежащих пользователю
    /// </summary>
    public List<InfoDirectory>? Directories { get; set; }

    /// <summary>
    /// Навигационное свойство для операций c файлами, выполненных пользователем
    /// </summary>
    public List<OperationFile>? OperationsFiles { get; set; }

    /// <summary>
    /// Навигационное свойство для операций c директориями, выполненных пользователем
    /// </summary>
    public List<OperationDirectory>? OperationsDirectories { get; set; }
}