namespace Reed;

public interface IResiliencyPolicy
{

}

public interface IOptimisticTimeoutPolicy : IResiliencyPolicy
{
    TimeSpan OptimisticTimeout { get; }
}

public interface IPessimisticTimeoutPolicy : IResiliencyPolicy
{
    TimeSpan PessimisticTimeout { get; }
}

public interface ICircuitBreakerTimeoutPolicy : IResiliencyPolicy
{
    double CircuitBreakerFailureThreshold { get; }
}

public interface IExceptionHandlingPolicy : IResiliencyPolicy
{
    bool HandleAllExceptions { get; }
}