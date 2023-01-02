using BenchmarkDotNet.Running;
using Reed.Benchmarks;

AppDomain.MonitoringIsEnabled = true;

var switcher = new BenchmarkSwitcher(new[] {
    typeof(ExceptionHandlingBenchmark),
    typeof(CompositePolicyBenchmark),
    typeof(CircuitBreakerBenchmark),
});

switcher.Run(args);

Console.ReadKey();