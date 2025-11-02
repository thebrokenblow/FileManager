namespace FileManager.Domain.Model;

public record DeleteDirectoryModel(
    DateTime ExecutedAt,
    string PathDirectory,
    int UserId,
    string[]? ChildLocationsFiles,
    string[]? ChildLocationsDirectories
);