namespace FileManager.Extensions;

public static class ListExtensions
{
    public static T Second<T>(this List<T> sequence)
    {
        if (sequence.Count < 2)
        {
            throw new InvalidOperationException("The sequence contains less than two elements");
        }

        return sequence[1];
    }

    public static T Third<T>(this List<T> sequence)
    {
        if (sequence.Count < 3)
        {
            throw new InvalidOperationException("The sequence contains less than two elements");
        }

        return sequence[2];
    }

    public static T Fourth<T>(this List<T> sequence)
    {
        if (sequence.Count < 4)
        {
            throw new InvalidOperationException("The sequence contains less than two elements");
        }

        return sequence[3];
    }

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