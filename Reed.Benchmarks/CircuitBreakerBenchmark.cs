using BenchmarkDotNet.Attributes;
using Polly;

namespace Reed.Benchmarks;

[MemoryDiagnoser]
[ExceptionDiagnoser]
[SimpleJob(launchCount: 2, warmupCount: 2, iterationCount: 6)]
public partial class CircuitBreakerBenchmark
{
    private IAsyncPolicy _pollyPolicy = Policy
        .Handle<Exception>()
        .AdvancedCircuitBreakerAsync(1, TimeSpan.FromDays(1), int.MaxValue, TimeSpan.FromSeconds(1));   

    public CircuitBreakerBenchmark()
    {
        
    }

    [GlobalSetup]
    public void Setup()
    {
        //_resiliencyPolicy = new ExceptionHandlingPolicy();
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
        }
    }
    
    [Resilient<IMyResiliencyPolicy>]
    public Task Reed2()
    {
        return MyTask();
    }
    
    public Task MyTask()
    {
        throw new TaskCanceledException();
    }
}