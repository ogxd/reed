using NUnit.Framework;

namespace Reed;

public partial class CircuitBreakerPolicyTests
{
    public CircuitBreakerPolicyTests()
    {

    }

    [SetUp]
    public void Setup()
    {
        _reedICircuitBreakerPolicy = new CircuitBreakerPolicy();
        _circuitBreakerThreshold1 = 0;
    }
    
    [Test]
    public async Task CircuitBreakerIncrementsOnFailure()
    {
        await InvalidOperation_Resilient();
        Assert.That(_circuitBreakerThreshold1, Is.EqualTo(1));
    }
    
    [Test]
    public async Task CircuitBreakerIncrementsUpToMaximumThreshold()
    {
        // We run many iterations because the circuit breaker increasingly throttle the call
        for (int i = 0; i < 100_000; i++)
        {
            await InvalidOperation_Resilient();
        }
        Assert.That(_circuitBreakerThreshold1, Is.EqualTo(_reedICircuitBreakerPolicy.CircuitBreakerFailureThreshold));
    }
    
    [Test]
    public async Task CircuitBreakerDoesNotHandle()
    {
        // We run many iterations because the circuit breaker increasingly throttle the call
        for (int i = 0; i < 100_000; i++)
        {
            await InvalidOperation_Resilient();
        }
        Assert.That(_circuitBreakerThreshold1, Is.EqualTo(_reedICircuitBreakerPolicy.CircuitBreakerFailureThreshold));
    }
    
    [Test]
    public void CircuitBreakerDoesNotHandlec()
    {
        _reedICircuitBreakerPolicy = new CircuitBreakerPolicy();
        Assert.DoesNotThrowAsync(InvalidOperation_Resilient);
        
        _reedICircuitBreakerPolicy = new TimeoutOnlyCircuitBreakerPolicy();
        Assert.ThrowsAsync<InvalidOperationException>(InvalidOperation_Resilient);
    }
    
    [Resilient<ICircuitBreakerPolicy>]
    private Task InvalidOperation()
    {
        throw new InvalidOperationException();
    }
}

public class CircuitBreakerPolicy : ICircuitBreakerPolicy
{
    public int CircuitBreakerFailureThreshold => 100;
    public bool IsExceptionHandled(Exception exception)
    {
        return true;
    }
}

public class TimeoutOnlyCircuitBreakerPolicy : ICircuitBreakerPolicy
{
    public int CircuitBreakerFailureThreshold => 100;
    public bool IsExceptionHandled(Exception exception)
    {
        return exception is TimeoutException;
    }
}


