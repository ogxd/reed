using BenchmarkDotNet.Attributes;
using Polly;
using Reed;

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
        
    }

    [GlobalSetup]
    public void Setup()
    {
        _resiliencyPolicy = new ExceptionHandlingPolicy();
    }
    
    [Benchmark]
    public Task Polly()
    {
        return _pollyPolicy.ExecuteAsync(MyTask);
    }
    
    [Resilient<IMyResiliencyPolicy>]
    public Task Reed()
    {
        return MyTask();
    }
    
    public Task MyTask()
    {
        throw new TaskCanceledException();
    }
}

public class ExceptionHandlingPolicy : IMyResiliencyPolicy
{
    public bool HandleAllExceptions => true;
}

public interface IMyResiliencyPolicy : IExceptionHandlingPolicy
{
}