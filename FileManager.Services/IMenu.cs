namespace FileManager.Services;

public interface IMenu
{
    int UserId { get; }
    string Path { get; }
    Action<string> Output { get; }
    Action<Action<string?>, string> InputText { get; }

    Task EnterCommand();
    void ChangePath(string path);
}