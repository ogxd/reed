namespace Reed;

public interface IOptimisticTimeoutPolicy : IResiliencyPolicy
{
    TimeSpan OptimisticTimeout { get; }
}