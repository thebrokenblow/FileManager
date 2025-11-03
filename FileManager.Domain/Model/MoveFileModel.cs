namespace FileManager.Domain.Model;

public record MoveFileModel(
    string PathSourceFile, 
    string NewPathSourceFile,
    DateTime ExecuteAt
);