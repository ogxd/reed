using BenchmarkDotNet.Attributes;
using Polly;

namespace Reed.Benchmarks;

[MemoryDiagnoser]
[ExceptionDiagnoser]
[SimpleJob]
public partial class ExceptionHandlingBenchmark
{
    private const int Iterations = 1;
    
    private IAsyncPolicy _pollyPolicy = Policy
        .Handle<Exception>()
        .FallbackAsync(_ => Task.CompletedTask);

    public ExceptionHandlingBenchmark()
    {
        _reedIExceptionHandlingPolicy = new ExceptionHandlingPolicy();
    }
    
    [Benchmark]
    public async Task Polly()
    {
        for (int i = 0; i < Iterations; i++)
        {
            await _pollyPolicy.ExecuteAsync(MyTask);
        }
    }
    
    [Benchmark]
    public async Task Reed()
    {
        for (int i = 0; i < Iterations; i++)
        {
            await ReedInternal_Resilient();
        }
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