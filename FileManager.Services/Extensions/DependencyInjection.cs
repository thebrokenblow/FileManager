using FileManager.Services.FileSystem.DirectoryAction.Persistent;
using FileManager.Services.FileSystem.DirectoryAction.Transient;
using FileManager.Services.FileSystem.Disck.Transient;
using FileManager.Services.FileSystem.FileAction.Persistent;
using FileManager.Services.FileSystem.FileAction.Transient;
using FileManager.Services.FileSystem.Other.Transient;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager.Services.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<CreateDirectoryAction>();
        services.AddTransient<DeleteDirectoryAction>();
        services.AddTransient<MoveDirectoriesAction>();

        services.AddTransient<ChangeDirectoryAction>();
        services.AddTransient<ShowDirectoriesAction>();

        services.AddTransient<InfoDisckAction>();

        services.AddTransient<ArchiveZipFilesAction>();
        services.AddTransient<CopyFileAction>();
        services.AddTransient<CreateFileAction>();
        services.AddTransient<DeleteFileAction>();
        services.AddTransient<MoveFileAction>();
        services.AddTransient<WriteFileAction>();

        services.AddTransient<DeserializeJsonFileAction>();
        services.AddTransient<DeserializeXmlFileAction>();
        services.AddTransient<ReadFileAction>();
        services.AddTransient<UnarchiveZipFilesAction>();

        services.AddTransient<HelpAction>();

        return services;
    }
}