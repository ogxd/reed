namespace Reed;

public class ExceptionHandlingPolicy : IMyResiliencyPolicy
{
    public TimeSpan OptimisticTimeout => TimeSpan.FromSeconds(1);
    public bool HandleAllExceptions => true;
}