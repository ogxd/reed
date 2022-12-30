using BenchmarkDotNet.Attributes;
using Polly;

namespace Reed.Benchmarks;

[MemoryDiagnoser]
[ExceptionDiagnoser]
[SimpleJob(launchCount: 2, warmupCount: 2, iterationCount: 6)]
public partial class ExceptionHandlingBenchmark
{
    private IAsyncPolicy _pollyPolicy = Policy
        .Handle<Exception>()
        .FallbackAsync(_ => Task.CompletedTask);

    public ExceptionHandlingBenchmark()
    {
        _resiliencyPolicy = new ExceptionHandlingPolicy();
    }
    
    [Benchmark]
    public Task Polly()
    {
        return _pollyPolicy.ExecuteAsync(MyTask);
    }
    
    [Benchmark]
    public Task Reed()
    {
        return ReedInternal_Resilient();
    }
    
    [Resilient<IExceptionHandlingPolicy>]
    public Task ReedInternal()
    {
        return MyTask();
    }
    
    public Task MyTask()
    {
        throw new TaskCanceledException();
    }
}

public class ExceptionHandlingPolicy : IExceptionHandlingPolicy
{
    public bool HandleAllExceptions => true;
}