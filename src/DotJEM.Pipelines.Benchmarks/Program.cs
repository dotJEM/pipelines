using System;
using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

#if DEBUG
BenchmarkSwitcher
    .FromAssembly(Assembly.GetEntryAssembly())
    .Run(args, new DebugInProcessConfig());
#else
Summary[] summary = BenchmarkRunner
    .Run(Assembly.GetEntryAssembly());
#endif


