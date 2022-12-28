namespace Reed;

public interface IMyResiliencyPolicy : IOptimisticTimeoutPolicy, IExceptionHandlingPolicy
{
    
}

public interface IMyResiliencyPolicy2 : IPessimisticTimeoutPolicy
{
    
}