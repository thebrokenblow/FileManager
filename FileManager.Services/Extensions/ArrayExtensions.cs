namespace FileManager.Services.Extensions;

public static class ArrayExtensions
{
    public static T Second<T>(this T[] sequence)
    {
        if (sequence.Length < 2)
        {
            throw new InvalidOperationException("The sequence contains less than two elements");
        }

        return sequence[1];
    }

    public static T Third<T>(this T[] sequence)
    {
        if (sequence.Length < 3)
        {
            throw new InvalidOperationException("The sequence contains less than two elements");
        }

        return sequence[2];
    }
}