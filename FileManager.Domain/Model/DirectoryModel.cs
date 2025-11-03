namespace FileManager.Domain.Model;

public record DirectoryModel(
    string DirectoryName, 
    string Location, 
    DateTime CreatedAt);