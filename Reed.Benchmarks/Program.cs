using BenchmarkDotNet.Running;
using Reed.Benchmarks;

AppDomain.MonitoringIsEnabled = true;

var switcher = new BenchmarkSwitcher(new[] {
    typeof(ExceptionHandlingBenchmark),
});

switcher.Run(args);

Console.ReadKey();