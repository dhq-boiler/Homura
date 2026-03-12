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
public class SelectBenchmark
{
    private string _dbPath = null!;
    private string _connectionString = null!;
    private readonly Guid _instanceId = Guid.NewGuid();

    [Params(100, 1000)]
    public int RecordCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"homura_bench_select_{Guid.NewGuid()}.db");
        _connectionString = $"Data Source={_dbPath}";

        // Setup via Homura migration
        ConnectionManager.SetDefaultConnection(_instanceId, _connectionString, typeof(SQLiteConnection));
        var svManager = new DataVersionManager();
        svManager.CurrentConnection = ConnectionManager.DefaultConnection;
        var plan = new ChangePlan<VersionOrigin>(VersioningMode.ByTick);
        plan.AddVersionChangePlan(new BenchmarkChangePlan(VersioningMode.ByTick));
        svManager.RegisterChangePlan(plan);
        svManager.SetDefault();
        svManager.UpgradeToTargetVersion();

        // Seed data
        var dao = new BenchmarkEntityDao(typeof(VersionOrigin));
        for (int i = 0; i < RecordCount; i++)
        {
            dao.InsertAsync(new BenchmarkEntity
            {
                Id = Guid.NewGuid(),
                Name = $"Item_{i}",
                Value = i,
                Description = $"Description for item {i}",
                CreatedAt = DateTime.UtcNow.ToString("O")
            }).GetAwaiter().GetResult();
        }

        ConnectionManager.DisposeDebris(_instanceId);
    }

    [IterationSetup]
    public void IterationSetup()
    {
        ConnectionManager.SetDefaultConnection(_instanceId, _connectionString, typeof(SQLiteConnection));
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        ConnectionManager.DisposeDebris(_instanceId);
        SqliteConnection.ClearAllPools();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        ConnectionManager.DisposeDebris(_instanceId);
        SqliteConnection.ClearAllPools();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { }
    }

    [Benchmark(Description = "Homura ORM")]
    public async Task<int> Homura_SelectAll()
    {
        var dao = new BenchmarkEntityDao(typeof(VersionOrigin));
        await using var conn = await dao.GetConnectionAsync();
        var results = await dao.FindAllAsync(conn);
        return results.Count;
    }

    [Benchmark(Description = "Homura ORM (Generated)")]
    public async Task<int> Homura_SelectAll_Generated()
    {
        var dao = new GeneratedBenchmarkEntityDao(typeof(VersionOrigin));
        await using var conn = await dao.GetConnectionAsync();
        var results = await dao.FindAllAsync(conn);
        return results.Count;
    }

    [Benchmark(Description = "Dapper")]
    public async Task<int> Dapper_SelectAll()
    {
        using var conn = new SQLiteConnection(_connectionString);
        await conn.OpenAsync();
        var results = (await conn.QueryAsync<EfBenchmarkEntity>(
            "SELECT Id, Name, Value, Description, CreatedAt FROM BenchmarkEntity")).ToList();
        return results.Count;
    }

    [Benchmark(Description = "EF Core")]
    public async Task<int> EfCore_SelectAll()
    {
        var options = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseSqlite($"Data Source={_dbPath};Pooling=False")
            .Options;
        using var context = new BenchmarkDbContext(options);
        var results = await context.BenchmarkEntities.ToListAsync();
        return results.Count;
    }
}
