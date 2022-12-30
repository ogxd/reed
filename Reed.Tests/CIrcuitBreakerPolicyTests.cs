using NUnit.Framework;

namespace Reed;

public partial class CircuitBreakerPolicyTests
{
    public CircuitBreakerPolicyTests()
    {
        _resiliencyPolicy = new CircuitBreakerPolicy();
    }
    
    [Test]
    public async Task CircuitBreakerIncrementsOnFailure()
    {
        Assert.That(_circuitBreakerThreshold, Is.EqualTo(0));
        await Resilient_Resilient();
        Assert.That(_circuitBreakerThreshold, Is.EqualTo(1));
    }
    
    [Test]
    public async Task CircuitBreakerIncrementsUpToMaximumThreshold()
    {
        // We run many iterations because the circuit breaker increasingly throttle the call
        for (int i = 0; i < 100_000; i++)
        {
            await Resilient_Resilient();
        }
        Assert.That(_circuitBreakerThreshold, Is.EqualTo(_resiliencyPolicy.CircuitBreakerFailureThreshold));
    }
    
    [Resilient<ICircuitBreakerPolicy>]
    public Task Resilient()
    {
        throw new InvalidOperationException();
    }
}

public class CircuitBreakerPolicy : ICircuitBreakerPolicy
{
    public int CircuitBreakerFailureThreshold => 100;
}

