namespace FileManager.Services.FileSystem.Other.Transient;

public readonly struct HelpCommand
{
    public required string Name { get; init; }
    public required string Usage { get; init; }
    public required string Description { get; init; }
}