namespace FileManager.Domain.Model;

public record WriteFileModel(
    string PathFile, 
    long FileSize, 
    DateTime ExecutedAt, 
    int UserId
);