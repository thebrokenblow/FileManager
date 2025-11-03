namespace FileManager.Domain.Model;

public record DeleteDirectoryModel(
    DateTime ExecutedAt,
    string PathDirectory,
    string[]? ChildLocationsFiles,
    string[]? ChildLocationsDirectories
);