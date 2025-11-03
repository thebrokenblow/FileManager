using FileManager.Domain.Interfaces.Queries;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Domain.Interfaces.UseCases;
using FileManager.Persistence.Data;
using FileManager.Persistence.Queries;
using FileManager.Persistence.Repositories;
using FileManager.Persistence.UseCases;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager.Persistence.Extensions;

public static class DependencyInjection
{
    /// <summary>
    /// Регистрирует сервисы уровня доступа к данным (Persistence layer)
    /// </summary>
    /// <param name="services">Коллекция сервисов для регистрации</param>
    /// <param name="connectionString">Строка подключения к базе данных</param>
    /// <returns>Коллекция сервисов с зарегистрированными зависимостями</returns>
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, string connectionString)
    {
        // Регистрация контекста базы данных
        services.AddDbContext<FileManagerContext>(options =>
            options.UseSqlServer(connectionString));

        // Регистрация query-сервисов для работы с данными
        services.AddScoped<IFileQueries, FileQueries>();
        services.AddScoped<IDirectoryQueries, DirectoryQueries>();

        // Регистрация репозиториев для базовых CRUD операций
        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<IDirectoryRepository, DirectoryRepository>();

        // Регистрация специализированных репозиториев для операций
        services.AddScoped<IOperationFileRepository, OperationFileRepository>();
        services.AddScoped<IOperationDirectoryRepository, OperationDirectoryRepository>();

        // Регистрация use case'ов (бизнес-сценариев) для работы с директориями
        services.AddScoped<IDirectoryUseCase, DirectoryUseCase>();
        services.AddScoped<IFileUseCase, FileUseCase>();

        return services;
    }
}