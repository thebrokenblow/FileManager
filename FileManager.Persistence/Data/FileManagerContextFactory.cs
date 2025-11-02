using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FileManager.Persistence.Data;

public class FileManagerContextFactory : IDesignTimeDbContextFactory<FileManagerContext>
{
    public FileManagerContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FileManagerContext>();
        optionsBuilder.UseSqlServer();

        var context = new FileManagerContext(optionsBuilder.Options);

        return context;
    }
}