using BenchmarkDotNet.Attributes;
using Polly;

namespace Reed.Benchmarks;

[MemoryDiagnoser]
[ExceptionDiagnoser]
[SimpleJob]
public partial class CircuitBreakerBenchmark
{
    private const int Iterations = 1;
    
    private IAsyncPolicy _pollyPolicy = Policy
        .Handle<Exception>()
        .AdvancedCircuitBreakerAsync(1, TimeSpan.FromDays(1), int.MaxValue, TimeSpan.FromSeconds(1));   

    public CircuitBreakerBenchmark()
    {
        _resiliencyPolicy = new CircuitBreakerPolicy();
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
            _circuitBreakerThreshold = 0;
            await ReedWrapped();
        }
    }
    
    [Resilient<ICircuitBreakerPolicy>("ReedWrapped")]
    private Task ReedInternal()
    {
        return MyTask();
    }

    private Task MyTask()
    {
        throw new TaskCanceledException();
    }
}

public class CircuitBreakerPolicy : ICircuitBreakerPolicy
{
    public int CircuitBreakerFailureThreshold => 100;
    
    public bool IsExceptionHandled(Exception exception)
    {
        return true;
    }
}