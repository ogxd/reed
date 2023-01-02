using BenchmarkDotNet.Attributes;
using Polly;

namespace Reed.Benchmarks;

[MemoryDiagnoser]
[ExceptionDiagnoser]
[SimpleJob]
public partial class CompositePolicyBenchmark
{
    private const int Iterations = 1;
    
    private IAsyncPolicy _pollyPolicy = Policy.WrapAsync(
        Policy
            .Handle<Exception>()
            .RetryAsync(3),
        Policy
            .Handle<Exception>()
            .AdvancedCircuitBreakerAsync(1, TimeSpan.FromDays(1), int.MaxValue, TimeSpan.FromSeconds(1)));   

    public CompositePolicyBenchmark()
    {
        _reedCompositePolicy = new CompositePolicy();
    }

    [Benchmark]
    public async Task Polly()
    {
        for (int i = 0; i < Iterations; i++)
        {
            try
            {
                await _pollyPolicy.ExecuteAsync(MyTask);
            }
            catch
            {
                // ignored
            }
        }
    }

    [Benchmark]
    public async Task Reed()
    {
        for (int i = 0; i < Iterations; i++)
        {
            // Hack to make sure Reed circuit breaker is always open
            _circuitBreakerThreshold1 = 0;
            await ReedWrapped();
        }
    }
    
    [Resilient<CompositePolicy>("ReedWrapped")]
    private Task ReedInternal()
    {
        return MyTask();
    }

    private Task MyTask()
    {
        throw new TaskCanceledException();
    }
}

public class CompositePolicy : ICircuitBreakerPolicy, IRetryPolicy
{
    public int CircuitBreakerFailureThreshold => 100;
    
    public bool IsExceptionHandled(Exception exception)
    {
        return true;
    }

    public int RetryAttempts => 3;
}