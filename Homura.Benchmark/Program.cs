using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Homura.Benchmark.Benchmarks;

var config = DefaultConfig.Instance;

Console.WriteLine("Homura ORM Benchmark - Homura vs Dapper vs EF Core");
Console.WriteLine("===================================================");
Console.WriteLine();
Console.WriteLine("Select benchmark to run:");
Console.WriteLine("  1. Insert");
Console.WriteLine("  2. Select");
Console.WriteLine("  3. Update");
Console.WriteLine("  4. FindBy");
Console.WriteLine("  5. All");
Console.WriteLine();

var key = args.Length > 0 ? args[0] : Console.ReadLine();

switch (key)
{
    case "1":
        BenchmarkRunner.Run<InsertBenchmark>(config);
        break;
    case "2":
        BenchmarkRunner.Run<SelectBenchmark>(config);
        break;
    case "3":
        BenchmarkRunner.Run<UpdateBenchmark>(config);
        break;
    case "4":
        BenchmarkRunner.Run<FindByBenchmark>(config);
        break;
    case "5":
    default:
        BenchmarkRunner.Run<InsertBenchmark>(config);
        BenchmarkRunner.Run<SelectBenchmark>(config);
        BenchmarkRunner.Run<UpdateBenchmark>(config);
        BenchmarkRunner.Run<FindByBenchmark>(config);
        break;
}
