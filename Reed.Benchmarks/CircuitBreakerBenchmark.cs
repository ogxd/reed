using BenchmarkDotNet.Attributes;
using Polly;

namespace Reed.Benchmarks;

[MemoryDiagnoser]
[ExceptionDiagnoser]
[SimpleJob]
public partial class CircuitBreakerBenchmark
{
    private IAsyncPolicy _pollyPolicy = Policy
        .Handle<Exception>()
        .AdvancedCircuitBreakerAsync(1, TimeSpan.FromDays(1), int.MaxValue, TimeSpan.FromSeconds(1));   

    public CircuitBreakerBenchmark()
    {
        _resiliencyPolicy = new CircuitBreakerPolicy();
    }

    [IterationSetup]
    public void Setup()
    {
        // Hack to make sure Reed circuit breaker is always open
        _circuitBreakerThreshold = 0;
    }

    [Benchmark]
    public async Task Polly()
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

    [Benchmark]
    public Task Reed()
    {
        return ReedWrapped();
    }
    
    [Resilient<ICircuitBreakerPolicy>("ReedWrapped")]
    public Task ReedInternal()
    {
        return MyTask();
    }
    
    public Task MyTask()
    {
        throw new TaskCanceledException();
    }
}

public class CircuitBreakerPolicy : ICircuitBreakerPolicy
{
    public int CircuitBreakerFailureThreshold => 100;
}