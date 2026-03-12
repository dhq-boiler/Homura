using System.Data.SQLite;
using BenchmarkDotNet.Attributes;
using Dapper;
using Homura.Benchmark.Entities;
using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Homura.Benchmark.Benchmarks;

[MemoryDiagnoser]
public class InsertBenchmark
{
    private string _dbPath = null!;
    private string _connectionString = null!;
    private readonly Guid _instanceId = Guid.NewGuid();

    [Params(1, 100, 1000)]
    public int RecordCount { get; set; }

    [IterationSetup]
    public void IterationSetup()
    {
        // Each iteration gets a fresh database
        _dbPath = Path.Combine(Path.GetTempPath(), $"homura_bench_insert_{Guid.NewGuid()}.db");
        _connectionString = $"Data Source={_dbPath}";

        // Create table via Homura migration
        ConnectionManager.SetDefaultConnection(_instanceId, _connectionString, typeof(SQLiteConnection));
        var svManager = new DataVersionManager();
        svManager.CurrentConnection = ConnectionManager.DefaultConnection;
        var plan = new ChangePlan<VersionOrigin>(VersioningMode.ByTick);
        plan.AddVersionChangePlan(new BenchmarkChangePlan(VersioningMode.ByTick));
        svManager.RegisterChangePlan(plan);
        svManager.SetDefault();
        svManager.UpgradeToTargetVersion();

        // Dispose so other ORMs can access
        ConnectionManager.DisposeDebris(_instanceId);
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        ConnectionManager.DisposeDebris(_instanceId);
        SqliteConnection.ClearAllPools();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { }
    }

    [Benchmark(Description = "Homura ORM")]
    public async Task Homura_Insert()
    {
        ConnectionManager.SetDefaultConnection(_instanceId, _connectionString, typeof(SQLiteConnection));
        var dao = new BenchmarkEntityDao(typeof(VersionOrigin));
        await using var conn = await dao.GetConnectionAsync();
        for (int i = 0; i < RecordCount; i++)
        {
            await dao.InsertAsync(new BenchmarkEntity
            {
                Id = Guid.NewGuid(),
                Name = $"Item_{i}",
                Value = i,
                Description = $"Description for item {i}",
                CreatedAt = DateTime.UtcNow.ToString("O")
            }, conn);
        }
    }

    [Benchmark(Description = "Homura ORM (Bulk)")]
    public async Task Homura_InsertBulk()
    {
        ConnectionManager.SetDefaultConnection(_instanceId, _connectionString, typeof(SQLiteConnection));
        var dao = new BenchmarkEntityDao(typeof(VersionOrigin));
        var entities = Enumerable.Range(0, RecordCount).Select(i => new BenchmarkEntity
        {
            Id = Guid.NewGuid(),
            Name = $"Item_{i}",
            Value = i,
            Description = $"Description for item {i}",
            CreatedAt = DateTime.UtcNow.ToString("O")
        });
        await dao.InsertBulkAsync(entities);
    }

    [Benchmark(Description = "Dapper")]
    public async Task Dapper_Insert()
    {
        using var conn = new SQLiteConnection(_connectionString);
        await conn.OpenAsync();
        for (int i = 0; i < RecordCount; i++)
        {
            await conn.ExecuteAsync(
                "INSERT INTO BenchmarkEntity (Id, Name, Value, Description, CreatedAt) VALUES (@Id, @Name, @Value, @Description, @CreatedAt)",
                new
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"Item_{i}",
                    Value = i,
                    Description = $"Description for item {i}",
                    CreatedAt = DateTime.UtcNow.ToString("O")
                });
        }
    }

    [Benchmark(Description = "EF Core")]
    public async Task EfCore_Insert()
    {
        var options = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseSqlite($"Data Source={_dbPath};Pooling=False")
            .Options;
        using var context = new BenchmarkDbContext(options);
        for (int i = 0; i < RecordCount; i++)
        {
            context.BenchmarkEntities.Add(new EfBenchmarkEntity
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"Item_{i}",
                Value = i,
                Description = $"Description for item {i}",
                CreatedAt = DateTime.UtcNow.ToString("O")
            });
        }
        await context.SaveChangesAsync();
    }
}
