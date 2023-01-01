# Reed

![ci workflow](https://github.com/ogxd/reed/actions/workflows/ci.yml/badge.svg)

> "The green reed which bends in the wind is stronger than the mighty oak which breaks in a storm"

## What is this?

A resiliency framework for dotnet

## Motivations

I like some concepts behind App-vNext/Polly however it must be said that:
- It sucks for high performance applications because it creates deep async state machine callstacks
  - More work of the task scheduler
  - Exceptions are rethrown many times
  - It makes it wtf to analyze in traces
- It's not easy to configure and naturally leads to a lot of heterogeneity in behaviour across I/Os in large applications
- It add a lot of visual complexity to the code with nested delegates in all places

The idea behind **Reed** is to create a resiliency framework based on source generators for maximum efficiency and easy and seamless integration.    

## Status

For now this is just a proof of concept ;)

### Usage

How it currently works is that you define a policy by implementing different features with provided interfaces.    
For instance, here is a policy that adds 3s timeouts and circuit breakers when too many timeout occurs:    
```csharp
public interface IMyCustomPolicy : ICircuitBreakerPolicy, IOptimisticTimeoutPolicy
{
}

public class MyCustomPolicy : IMyCustomPolicy
{
    public int CircuitBreakerFailureThreshold => 100;
    
    public bool IsExceptionHandled(Exception exception)
    {
        return exception is TimeoutException;
    }
    
    public TimeSpan OptimisticTimeout => TimeSpan.FromSeconds(3);
}
```
Then you must add the `[Resilient<IMyCustomPolicy>]` to the method you want to make resilient. That will automatically create a new method (with the name of you method with the _Resilient suffix, but you can have a custom name if you want). This new method will be generated with appropriate code to handle timeouts and circuit breaking.    
You must also make its containing class partial for codegen to work and pass the policy to the new constructor or even better register the policy implementation with dependency injection.

## Ideas

- [x] Custom method names
- [x] Circuit breaker policy (lock-free...)
- [x] Benchmark against polly
- [ ] Optimistic timeout policy
- [ ] Pessimistic timeout policy
- [ ] Retry policy
- [ ] Pluggable callback interfaces over unitary policies (eg to monitor circuit breaker)

## Benchmark

As a small example, here is a small benchmark of Polly VS Reed for exception handling:    
| Method |     Mean |    Error |   StdDev |   Gen0 | Exceptions | Allocated |
|------- |---------:|---------:|---------:|-------:|-----------:|----------:|
|  Polly | 31.86 us | 0.260 us | 0.203 us | 1.1597 |     2.0000 |    2496 B |
|   Reed | 14.85 us | 0.311 us | 0.243 us | 0.1678 |     1.0000 |     360 B |
    
An now a benchmark of Polly Advanced Circuit Breakers VS Reed Circuit Breaker (code generated)
| Method |      Mean |    Error |   StdDev |   Gen0 | Exceptions | Allocated |
|------- |----------:|---------:|---------:|-------:|-----------:|----------:|
|  Polly | 131.22 us | 0.527 us | 0.467 us | 4.1504 |     7.0000 |    9120 B |
|   Reed |  14.58 us | 0.082 us | 0.076 us | 0.1678 |     1.0000 |     360 B |

7 exceptions thrown with Polly and 9kb allocated per iteration 💀🤯
