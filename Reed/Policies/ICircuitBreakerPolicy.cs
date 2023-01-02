namespace Reed;

public interface ICircuitBreakerPolicy : IExceptionHandlingPolicy
{
    int CircuitBreakerFailureThreshold { get; }
}