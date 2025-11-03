namespace FileManager.Domain.Model;

public record FileModel(
    string FileName, 
    string FullPath, 
    DateTime CreateAt, 
    long Size
);