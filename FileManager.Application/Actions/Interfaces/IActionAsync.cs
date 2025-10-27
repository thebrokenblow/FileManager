namespace FileManager.Application.Actions.Interfaces;

public interface IActionAsync
{
    Task ExecuteAsync(string command);
}