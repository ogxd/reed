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