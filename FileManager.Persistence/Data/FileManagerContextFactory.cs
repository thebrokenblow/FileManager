using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FileManager.Persistence.Data;

public class FileManagerContextFactory(string connectionString) : IDesignTimeDbContextFactory<FileManagerContext>
{
    public FileManagerContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FileManagerContext>();
        optionsBuilder.UseNpgsql(connectionString);

        var context = new FileManagerContext(optionsBuilder.Options);

        return context;
    }
}