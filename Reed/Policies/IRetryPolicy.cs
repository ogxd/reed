namespace Reed;

public interface IRetryPolicy : IExceptionHandlingPolicy
{
    int RetryAttempts { get; }
}