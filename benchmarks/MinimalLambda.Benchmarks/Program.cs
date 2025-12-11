using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using MinimalLambda.Benchmarks;

var config = DefaultConfig.Instance;

var summary = BenchmarkRunner.Run<CreateBuilderBenchmarks>(config, args);

// var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
