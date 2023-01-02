namespace Reed;

public interface IExceptionHandlingPolicy : IResiliencyPolicy
{
    bool IsExceptionHandled(Exception exception);
}