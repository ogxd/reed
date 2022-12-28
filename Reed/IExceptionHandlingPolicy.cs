namespace Reed;

public interface IExceptionHandlingPolicy : IResiliencyPolicy
{
    bool HandleAllExceptions { get; }
}