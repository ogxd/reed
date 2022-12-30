namespace Reed;

public static class ReedExtensions
{
    public static int IncrementClamped(ref int value, int max)
    {
        int initial = Thread.VolatileRead(ref value);
        if (initial >= max)
        {
            return max;
        }
        return Interlocked.Increment(ref value);
    }
    
    public static int DecrementClamped(ref int value, int min)
    {
        int initial = Thread.VolatileRead(ref value);
        if (initial <= min)
        {
            return min;
        }
        return Interlocked.Decrement(ref value);
    }
}