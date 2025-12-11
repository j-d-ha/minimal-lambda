using BenchmarkDotNet.Attributes;
using MinimalLambda.Host.Builder;

namespace MinimalLambda.Benchmarks;

public class CreateBuilderBenchmarks
{
    [Benchmark]
    public void CreateBuilder() => LambdaApplication.CreateBuilder();

    [Benchmark]
    public void CreateBuilderDisableDefaults() =>
        LambdaApplication.CreateBuilder(new LambdaApplicationOptions { DisableDefaults = true });
}
