using FileManager.Data.Entities;

namespace FileManager.Data.Repositories;

public class InfoFileRepository(FileManagerContext context)
{
    public void Add(InfoFile infoFile)
    {
        context.Add(infoFile);
        context.SaveChanges();
    }
}