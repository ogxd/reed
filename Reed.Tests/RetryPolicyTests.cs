using NUnit.Framework;

namespace Reed;

public partial class RetryPolicyTests
{
    private int _countedAttempts;
    
    public RetryPolicyTests()
    {
        _reedRetryPolicy = new RetryPolicy();
        _countedAttempts = 0;
    }

    [Test]
    public async Task RetriesUpToMaximumAttempts()
    {
        Assert.That(_countedAttempts, Is.EqualTo(0));
        await InvalidOperation_Resilient();
        Assert.That(_countedAttempts, Is.EqualTo(_reedRetryPolicy.RetryAttempts + 1)); // Retries + Initial attempt
    }
    
    [Resilient<RetryPolicy>]
    private Task InvalidOperation()
    {
        _countedAttempts++;
        throw new InvalidOperationException();
    }
}

public class RetryPolicy : ICircuitBreakerPolicy, IRetryPolicy
{
    public int CircuitBreakerFailureThreshold => 1;
    public bool IsExceptionHandled(Exception exception)
    {
        return true;
    }

    public int RetryAttempts => 2;
}


