namespace Reed;

public interface ICircuitBreakerPolicy : IResiliencyPolicy
{
    int CircuitBreakerFailureThreshold { get; }
    bool IsExceptionHandled(Exception exception);
}