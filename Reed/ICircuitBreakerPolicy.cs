namespace Reed;

public interface ICircuitBreakerPolicy : IResiliencyPolicy
{
    int CircuitBreakerFailureThreshold { get; }
}