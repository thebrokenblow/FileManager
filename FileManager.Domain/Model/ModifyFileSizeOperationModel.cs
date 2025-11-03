namespace FileManager.Domain.Model;

public record ModifyFileSizeOperationModel(
    string PathFile, 
    long FileSize, 
    DateTime ExecutedAt, 
    int UserId);