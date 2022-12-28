namespace Reed;

public interface ICircuitBreakerTimeoutPolicy : IResiliencyPolicy
{
    double CircuitBreakerFailureThreshold { get; }
}