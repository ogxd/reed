using NUnit.Framework;

namespace Reed;

public partial class CircuitBreakerPolicyTests
{
    public CircuitBreakerPolicyTests()
    {
        _resiliencyPolicy = new CircuitBreakerPolicy();
    }
    
    [Test]
    public async Task NonResilient()
    {
        Assert.That(_circuitBreakerThreshold, Is.EqualTo(0));
        await Resilient_Resilient();
        Assert.That(_circuitBreakerThreshold, Is.EqualTo(1));
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

