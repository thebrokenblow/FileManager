namespace FileManager.Domain.Model;

public record MoveDirectoryModel(
    DateTime ExecutedAt,
    string PathSourceDirectory,
    string PathDestinationDirectory,
    string NewFullPathDestinationDirectory,
    int UserId
);