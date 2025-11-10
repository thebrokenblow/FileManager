using FileManager.Services;
using FileManager.Services.FileSystem.Other.Transient;

namespace FileManager.UI.ViewUI;

public class Menu : IMenu
{
    public const string TitleUserDirectory = "UserDirectory";

    public int UserId { get; set; }
    public required string Path { get; set; }

    public required Action<string> Output { get; init; }
    public required Action<HelpInfo> OutputHelpPanel { get; init; }
    public required Action<Action<string?>, string> InputText { get; init; }
}