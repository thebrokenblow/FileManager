using FileManager.Services.FileSystem.Other.Transient;

namespace FileManager.Services;

public interface IMenu
{
    int UserId { get; set; }
    string Path { get; set; }
    Action<string> Output { get; }
    Action<HelpInfo> OutputHelpPanel { get; }
    Action<Action<string?>, string> InputText { get; }

    //void ChangePath(string path);
}