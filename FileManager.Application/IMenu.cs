namespace FileManager.Application;

public interface IMenu
{
    public int UserId { get; }
    public string Path { get; }
    public Action<string> Output { get; }

    public void ChangePath(string path);
}