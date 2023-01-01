namespace Reed;

public static class ReedExtensions
{
    /// <summary>
    /// A lock-free solution for atomic (composite) clamped increment
    /// </summary>
    /// <param name="value"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static int IncrementClamped(ref int value, int max)
    {
        while (true)
        {
            int initial = Thread.VolatileRead(ref value);
            if (initial >= max)
            {
                return max;
            }
            int computed = initial + 1;
            // If initial value is still what it was before, it means no other thread has changed it, thus we can consider the operation successful and return
            if (Interlocked.CompareExchange(ref value, computed, initial) == initial)
            {
                return value;
            }
            // Retry
        }
    }
    
    /// <summary>
    /// A lock-free solution for atomic (composite) clamped decrement
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <returns></returns>
    public static int DecrementClamped(ref int value, int min)
    {
        while (true)
        {
            int initial = Thread.VolatileRead(ref value);
            if (initial <= min)
            {
                return min;
            }
            int computed = initial - 1;
            // If initial value is still what it was before, it means no other thread has changed it, thus we can consider the operation successful and return
            if (Interlocked.CompareExchange(ref value, computed, initial) == initial)
            {
                return value;
            }
            // Retry
        }
    }
}