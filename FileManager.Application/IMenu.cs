namespace FileManager.Application;

public interface IMenu
{
    int UserId { get; }
    string Path { get; }
    Action<string> Output { get; }
    Action<Action<string>, string> InputText { get; }

    void ChangePath(string path);
}