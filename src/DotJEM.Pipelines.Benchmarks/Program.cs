using System;
using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using DotJEM.Pipelines.Benchmarks;

#if DEBUG
BenchmarkSwitcher
    .FromAssembly(Assembly.GetEntryAssembly())
    .Run(args, new DebugInProcessConfig());
#else
Summary summary = BenchmarkRunner
    .Run<PipelineExecutionWithoutLoggerBenchmarks>();
    //.Run(Assembly.GetEntryAssembly());
#endif


