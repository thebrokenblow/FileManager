namespace FileManager.Services.Extensions;

public static class ListExtensions
{
    public static bool IsEmpty<T>(this List<T> sequence)
    {
        return sequence.Count == 0;    
    }

    public static List<T> RemoveLast<T>(this List<T> sequence)
    {
        if (sequence.Count > 0)
        {
            sequence.RemoveAt(sequence.Count - 1);
        }

        return sequence;
    }
}