using FileManager.Services.Utils;

namespace FileManager.Services.Validation;

public class CommandValidator
{
    public CommandValidator ValidateNotEmpty(string source, string textError)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException(textError);
        }

        return this;
    }

    public CommandValidator ValidateArgumentsCount(out string[] arguments, string command, int expectedCount, string textError)
    {
        arguments = command.Split(WhitespaceCharsDictionary.AllWhitespace, StringSplitOptions.RemoveEmptyEntries);

        if (arguments.Length != expectedCount)
        {
            throw new ArgumentException(textError);
        }

        return this;
    }

    public CommandValidator ValidateCommandName(string actualCommand, string expectedCommand, string textError)
    {
        if (!actualCommand.Equals(expectedCommand, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException(textError);
        }

        return this;
    }

    public CommandValidator ValidatePathSecurity(string fileOrDirectoryName, string textError)
    {
        if (PathSecurity.IsPathTraversal(fileOrDirectoryName))
        {
            throw new ArgumentException(textError);
        }

        return this;
    }

    public CommandValidator ValidateDirectoryExists(string fullPath, string textError)
    {
        if (!Directory.Exists(fullPath))
        {
            throw new ArgumentException(textError);
        }

        return this;
    }

    public CommandValidator ValidateDirectoryNotExists(string fullPath, string textError)
    {
        if (Directory.Exists(fullPath))
        {
            throw new ArgumentException(textError);
        }

        return this;
    }

    public CommandValidator ValidateFileExists(string fullPath, string textError)
    {
        if (!File.Exists(fullPath))
        {
            throw new ArgumentException(textError);
        }

        return this;
    }

    public CommandValidator ValidateFileNotExists(string fullPath, string textError)
    {
        if (File.Exists(fullPath))
        {
            throw new ArgumentException(textError);
        }

        return this;
    }

    public CommandValidator ValidateCustom(Func<bool> validation, string errorMessage)
    {
        if (validation.Invoke())
        {
            throw new ArgumentException(errorMessage);
        }

        return this;
    }
}