using BenchmarkDotNet.Running;
using GdsSharp.Benchmarks.Obsolete;
using GdsSharp.Lib.Reading;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);