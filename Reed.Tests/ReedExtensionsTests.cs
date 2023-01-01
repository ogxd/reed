using NUnit.Framework;

namespace Reed;

public class ReedExtensionsTests
{
    [Test]
    public async Task IncrementIsThreadSafe()
    {
        int value = 0;
        const int max = 50_000; // Shall not be reached
        var tasks = Enumerable.Range(0, 4).Select(j => Task.Run(async () =>
        {
            await Task.Yield();
            for (int i = 0; i < 10_000; i++)
            {
                ReedExtensions.IncrementClamped(ref value, max);
            }
        }));

        await Task.WhenAll(tasks);
        
        // Verifies that the value was incremented excactly the requested number of times,
        // despite being done from several threads.
        Assert.That(value, Is.EqualTo(4 * 10_000));
    }
    
    [Test]
    public async Task DoesNotGetAboveMax()
    {
        int value = 0;
        const int parallism = 12;
        const int iterationsPerThread = 100_000;
        const int max = parallism * iterationsPerThread / 2;
        var tasks = Enumerable.Range(0, parallism).Select(j => Task.Run(async () =>
        {
            await Task.Yield();
            for (int i = 0; i < iterationsPerThread; i++)
            {
                int valueCopy = value;
                int incrementedValue = ReedExtensions.IncrementClamped(ref value, max);
                Assert.LessOrEqual(incrementedValue, max);
                Assert.GreaterOrEqual(incrementedValue, valueCopy, $"Value was {valueCopy} and is {incrementedValue} after being incremented, which shouldn't be possible");
            }
        }));

        await Task.WhenAll(tasks);
        
        Assert.That(value, Is.EqualTo(max));
    }
}
