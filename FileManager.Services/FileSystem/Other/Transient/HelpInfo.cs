namespace FileManager.Services.FileSystem.Other.Transient;

public readonly struct HelpInfo
{
    public required string Title { get; init; }
    public required string TitleNameCommand { get; init; }
    public required string TitleUsageCommand { get; init; }
    public required string TitleDescriptionCommand { get; init; }


    public required string TitleDirectoryChapter { get; init; }
    public required List<HelpCommand> HelpDirectoryCommands { get; init; }

    public required string TitleFileChapter { get; init; }
    public required List<HelpCommand> HelpFileCommands { get; init; }

    public required string TitleUtilityChapter { get; init; }
    public required List<HelpCommand> HelpUtilityCommands { get; init; }
}