namespace Reed;

public interface IPessimisticTimeoutPolicy : IResiliencyPolicy
{
    TimeSpan PessimisticTimeout { get; }
}