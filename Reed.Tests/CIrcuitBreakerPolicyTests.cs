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
        _resiliencyPolicy = new CircuitBreakerPolicy();
        _circuitBreakerThreshold = 0;
    }
    
    [Test]
    public async Task CircuitBreakerIncrementsOnFailure()
    {
        await InvalidOperation_Resilient();
        Assert.That(_circuitBreakerThreshold, Is.EqualTo(1));
    }
    
    [Test]
    public async Task CircuitBreakerIncrementsUpToMaximumThreshold()
    {
        // We run many iterations because the circuit breaker increasingly throttle the call
        for (int i = 0; i < 100_000; i++)
        {
            await InvalidOperation_Resilient();
        }
        Assert.That(_circuitBreakerThreshold, Is.EqualTo(_resiliencyPolicy.CircuitBreakerFailureThreshold));
    }
    
    [Test]
    public async Task CircuitBreakerDoesNotHandle()
    {
        // We run many iterations because the circuit breaker increasingly throttle the call
        for (int i = 0; i < 100_000; i++)
        {
            await InvalidOperation_Resilient();
        }
        Assert.That(_circuitBreakerThreshold, Is.EqualTo(_resiliencyPolicy.CircuitBreakerFailureThreshold));
    }
    
    [Test]
    public void CircuitBreakerDoesNotHandlec()
    {
        _resiliencyPolicy = new CircuitBreakerPolicy();
        Assert.DoesNotThrowAsync(InvalidOperation_Resilient);
        
        _resiliencyPolicy = new TimeoutOnlyCircuitBreakerPolicy();
        Assert.ThrowsAsync<InvalidOperationException>(InvalidOperation_Resilient);
    }
    
    [Resilient<ICircuitBreakerPolicy>]
    private Task InvalidOperation()
    {
        throw new InvalidOperationException();
    }
    
    // TODO: Handle multiple resilient methods per class
    // [Resilient<ICircuitBreakerPolicy>]
    // private Task TimeoutOperation()
    // {
    //     throw new TimeoutException();
    // }
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


