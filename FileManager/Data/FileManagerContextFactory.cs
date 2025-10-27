using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FileManager.Data;

public class FileManagerContextFactory : IDesignTimeDbContextFactory<FileManagerContext>
{
    public FileManagerContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Configurations\\appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string connectionString = configuration.GetConnectionString("DbConnection")
                                        ?? throw new InvalidOperationException("Connection string not found");

        var optionsBuilder = new DbContextOptionsBuilder<FileManagerContext>();
        optionsBuilder.UseSqlServer(connectionString);

        var context = new FileManagerContext(optionsBuilder.Options);

        return context;
    }
}