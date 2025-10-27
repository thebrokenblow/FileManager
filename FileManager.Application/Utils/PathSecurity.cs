namespace FileManager.Application.Utils;

public static class PathSecurity
{
    public static bool IsPathTraversal(string inputPath)
    {
        if (string.IsNullOrEmpty(inputPath))
        {
            return false;
        }

        return inputPath.Contains("..") ||
               inputPath.Contains('~') ||
               inputPath.StartsWith('.') ||
               inputPath.StartsWith(' ') ||
               inputPath.EndsWith('.') ||
               inputPath.EndsWith(' ') ||
               inputPath.Contains('/') ||
               inputPath.Contains('\\') ||
               inputPath.Contains(':') ||
               Path.IsPathRooted(inputPath) ||
               inputPath.Contains("//") ||
               inputPath.Contains("\\\\");
    }
}