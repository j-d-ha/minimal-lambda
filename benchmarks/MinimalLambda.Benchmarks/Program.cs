using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using MinimalLambda.Benchmarks;

var config = DefaultConfig.Instance;

BenchmarkRunner.Run<CreateBuilderBenchmarks>(config, args);
