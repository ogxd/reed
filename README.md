# Reed

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

## Ideas

- [ ] Custom method names
- [ ] ExceptionHandling
- [ ] Circuit breaker (lock-free...)
- [ ] Optimistic timeout
- [ ] Pessimistic timeout
- [ ] Pluggable callback interfaces over unitary policies (eg to monitor circuit breaker)
- [ ] Benchmark against polly

## Benchmark

As a small example, here is a small benchmark of Polly VS Reed for exception handling:    
|         Method |     Mean |    Error |   StdDev |   Gen0 | Exceptions | Allocated |
|--------------- |---------:|---------:|---------:|-------:|-----------:|----------:|
|          Polly | 31.86 us | 0.260 us | 0.203 us | 1.1597 |     2.0000 |    2496 B |
| Reed_Resilient | 14.85 us | 0.311 us | 0.243 us | 0.1678 |     1.0000 |     360 B |
