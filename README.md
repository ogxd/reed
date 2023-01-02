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
- [x] Retry policy
- [ ] Optimistic timeout policy
- [ ] Pessimistic timeout policy
- [ ] Pluggable callback interfaces over unitary policies (eg to monitor circuit breaker)

## Benchmark

As a small example, here is benchmark of Polly Advanced Circuit Breakers VS Reed Circuit Breaker (code generated)
| Method |      Mean |    Error |   StdDev |   Gen0 | Exceptions | Allocated |
|------- |----------:|---------:|---------:|-------:|-----------:|----------:|
|  Polly | 131.22 us | 0.527 us | 0.467 us | 4.1504 |     7.0000 |    9120 B |
|   Reed |  14.58 us | 0.082 us | 0.076 us | 0.1678 |     1.0000 |     360 B |

7 exceptions thrown with Polly and 9kb allocated per iteration 💀🤯

Here is another example this time with Circuit Breaker + Retry policy
| Method |      Mean |    Error |   StdDev |    Gen0 | Exceptions | Allocated |
|------- |----------:|---------:|---------:|--------:|-----------:|----------:|
|  Polly | 765.31 us | 5.920 us | 5.538 us | 37.1094 |    41.0000 |     76 KB |
|   Reed |  57.94 us | 0.197 us | 0.184 us |  0.6714 |     4.0000 |   1.41 KB |

13x less CPU time and 10x less exceptions with Reed

### Callstack

#### With Polly

```
CompositePolicyBenchmark.MyTask()at /Users/olivierginiaux/src/perso/reed/Reed.Benchmarks/CompositePolicyBenchmark.cs:line 61
AsyncPolicy.<>c__DisplayClass4_0.<ExecuteAsync>b__0()
AsyncPolicy.<>c__DisplayClass40_0.<<ImplementationAsync>b__0>d.MoveNext() [2]
AsyncMethodBuilderCore.Start<Polly.AsyncPolicy.<>c__DisplayClass40_0.<<ImplementationAsync>b__0>d>() [2]
AsyncTaskMethodBuilder<EmptyStruct>.Start<Polly.AsyncPolicy.<>c__DisplayClass40_0.<<ImplementationAsync>b__0>d>() [2]
AsyncPolicy.<>c__DisplayClass40_0.<ImplementationAsync>b__0() [2]
AsyncCircuitBreakerPolicy.<>c__DisplayClass8_0<EmptyStruct>.<<ImplementationAsync>b__0>d.MoveNext()
AsyncMethodBuilderCore.Start<Polly.CircuitBreaker.AsyncCircuitBreakerPolicy.<>c__DisplayClass8_0<Polly.Utilities.EmptyStruct>.<<ImplementationAsync>b__0>d>()
AsyncTaskMethodBuilder<EmptyStruct>.Start<Polly.CircuitBreaker.AsyncCircuitBreakerPolicy.<>c__DisplayClass8_0<Polly.Utilities.EmptyStruct>.<<ImplementationAsync>b__0>d>()
AsyncCircuitBreakerPolicy.<>c__DisplayClass8_0<EmptyStruct>.<ImplementationAsync>b__0()
AsyncCircuitBreakerEngine.<ImplementationAsync>d__0<EmptyStruct>.MoveNext()
AsyncMethodBuilderCore.Start<Polly.CircuitBreaker.AsyncCircuitBreakerEngine.<ImplementationAsync>d__0<Polly.Utilities.EmptyStruct>>()
AsyncTaskMethodBuilder<EmptyStruct>.Start<Polly.CircuitBreaker.AsyncCircuitBreakerEngine.<ImplementationAsync>d__0<Polly.Utilities.EmptyStruct>>()
AsyncCircuitBreakerEngine.ImplementationAsync<Polly.Utilities.EmptyStruct>()
AsyncCircuitBreakerPolicy.<ImplementationAsync>d__8<EmptyStruct>.MoveNext()
AsyncMethodBuilderCore.Start<Polly.CircuitBreaker.AsyncCircuitBreakerPolicy.<ImplementationAsync>d__8<Polly.Utilities.EmptyStruct>>()
AsyncTaskMethodBuilder<EmptyStruct>.Start<Polly.CircuitBreaker.AsyncCircuitBreakerPolicy.<ImplementationAsync>d__8<Polly.Utilities.EmptyStruct>>()
AsyncCircuitBreakerPolicy.ImplementationAsync<Polly.Utilities.EmptyStruct>()
AsyncPolicy.ImplementationAsync() [2]
AsyncPolicy.<ExecuteAsync>d__12.MoveNext() [3]
AsyncMethodBuilderCore.Start<Polly.AsyncPolicy.<ExecuteAsync>d__12>() [3]
AsyncTaskMethodBuilder.Start<Polly.AsyncPolicy.<ExecuteAsync>d__12>() [3]
AsyncPolicy.ExecuteAsync() [3]
AsyncPolicyWrapEngine.<>c__DisplayClass4_0.<<ImplementationAsync>b__0>d.MoveNext()
AsyncMethodBuilderCore.Start<Polly.Wrap.AsyncPolicyWrapEngine.<>c__DisplayClass4_0.<<ImplementationAsync>b__0>d>()
AsyncTaskMethodBuilder.Start<Polly.Wrap.AsyncPolicyWrapEngine.<>c__DisplayClass4_0.<<ImplementationAsync>b__0>d>()
AsyncPolicyWrapEngine.<>c__DisplayClass4_0.<ImplementationAsync>b__0()
AsyncPolicy.<>c__DisplayClass40_0.<<ImplementationAsync>b__0>d.MoveNext() [1]
AsyncMethodBuilderCore.Start<Polly.AsyncPolicy.<>c__DisplayClass40_0.<<ImplementationAsync>b__0>d>() [1]
AsyncTaskMethodBuilder<EmptyStruct>.Start<Polly.AsyncPolicy.<>c__DisplayClass40_0.<<ImplementationAsync>b__0>d>() [1]
AsyncPolicy.<>c__DisplayClass40_0.<ImplementationAsync>b__0() [1]
AsyncRetryEngine.<ImplementationAsync>d__0<EmptyStruct>.MoveNext()
AsyncMethodBuilderCore.Start<Polly.Retry.AsyncRetryEngine.<ImplementationAsync>d__0<Polly.Utilities.EmptyStruct>>()
AsyncTaskMethodBuilder<EmptyStruct>.Start<Polly.Retry.AsyncRetryEngine.<ImplementationAsync>d__0<Polly.Utilities.EmptyStruct>>()
AsyncRetryEngine.ImplementationAsync<Polly.Utilities.EmptyStruct>()
AsyncRetryPolicy.ImplementationAsync<Polly.Utilities.EmptyStruct>()
AsyncPolicy.ImplementationAsync() [1]
AsyncPolicy.<ExecuteAsync>d__12.MoveNext() [2]
AsyncMethodBuilderCore.Start<Polly.AsyncPolicy.<ExecuteAsync>d__12>() [2]
AsyncTaskMethodBuilder.Start<Polly.AsyncPolicy.<ExecuteAsync>d__12>() [2]
AsyncPolicy.ExecuteAsync() [2]
AsyncPolicyWrapEngine.<ImplementationAsync>d__4.MoveNext()
AsyncMethodBuilderCore.Start<Polly.Wrap.AsyncPolicyWrapEngine.<ImplementationAsync>d__4>()
AsyncTaskMethodBuilder.Start<Polly.Wrap.AsyncPolicyWrapEngine.<ImplementationAsync>d__4>()
AsyncPolicyWrapEngine.ImplementationAsync()
AsyncPolicyWrap.ImplementationAsync()
AsyncPolicy.<ExecuteAsync>d__12.MoveNext() [1]
AsyncMethodBuilderCore.Start<Polly.AsyncPolicy.<ExecuteAsync>d__12>() [1]
AsyncTaskMethodBuilder.Start<Polly.AsyncPolicy.<ExecuteAsync>d__12>() [1]
AsyncPolicy.ExecuteAsync() [1]
AsyncPolicy.ExecuteAsync()
Program.<Main>$()
```

#### With Reed

```
CompositePolicyBenchmark.MyTask()at /Users/olivierginiaux/src/perso/reed/Reed.Benchmarks/CompositePolicyBenchmark.cs:line 61
CompositePolicyBenchmark.ReedInternal()at /Users/olivierginiaux/src/perso/reed/Reed.Benchmarks/CompositePolicyBenchmark.cs:line 56
async CompositePolicyBenchmark.ReedWrapped()
AsyncMethodBuilderCore.Start<Reed.Benchmarks.CompositePolicyBenchmark.<ReedWrapped>d__10>()
AsyncTaskMethodBuilder.Start<Reed.Benchmarks.CompositePolicyBenchmark.<ReedWrapped>d__10>()
CompositePolicyBenchmark.ReedWrapped()
Program.<Main>$()
```