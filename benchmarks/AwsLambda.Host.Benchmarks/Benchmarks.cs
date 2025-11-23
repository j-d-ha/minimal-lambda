using AwsLambda.Host.Builder;
using BenchmarkDotNet.Attributes;

namespace AwsLambda.Host.Benchmarks;

public class CreateBuilderBenchmarks
{
    [Benchmark]
    public void CreateBuilder() => LambdaApplication.CreateBuilder();

    [Benchmark]
    public void CreateBuilderDisableDefaults() =>
        LambdaApplication.CreateBuilder(new LambdaApplicationOptions { DisableDefaults = true });
}
