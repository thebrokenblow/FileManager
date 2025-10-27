using FileManager.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FileManager.Data;

/// <summary>
/// Контекст базы данных для файлового менеджера
/// </summary>
public class FileManagerContext(DbContextOptions<FileManagerContext> options) : DbContext(options)
{
    /// <summary>
    /// Набор данных файлов
    /// </summary>
    public DbSet<InfoFile> InfoFiles { get; set; }

    /// <summary>
    /// Набор данных директорий
    /// </summary>
    public DbSet<InfoDirectory> InfoDirectories { get; set; }

    /// <summary>
    /// Набор данных операций с директориями
    /// </summary>
    public DbSet<OperationDirectory> DirectoryOperations { get; set; }

    /// <summary>
    /// Набор данных операций с файлами
    /// </summary>
    public DbSet<OperationFile> FileOperations { get; set; }

    /// <summary>
    /// Набор данных пользователей
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Настраивает модель базы данных
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}