using System;
using System.Threading;

public static class Counter
{
    public const int MaxCount = 2;

    private static long counter;

    public static void Increment()
    {
        var count = Interlocked.Increment(ref counter);
        if (count > MaxCount)
        {
            throw new InvalidOperationException(
                $"The number of concurrent tests ({counter}) is greater than allowed ({MaxCount}).");
        }
    }

    public static void Decrement()
    {
        Interlocked.Decrement(ref counter);
    }
}
