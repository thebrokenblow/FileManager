using FileManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace FileManager.Persistence.Data;

/// <summary>
/// Контекст базы данных для файлового менеджера
/// </summary>
public class FileManagerContext : DbContext
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


    public FileManagerContext() { }

    public FileManagerContext(DbContextOptions<FileManagerContext> options) : base(options) { }


    /// <summary>
    /// Настраивает модель базы данных
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Configurations/appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DbConnection");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}