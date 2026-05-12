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
public class FindByBenchmark
{
    private string _dbPath = null!;
    private string _connectionString = null!;
    private readonly Guid _instanceId = Guid.NewGuid();
    private Guid[] _ids = null!;

    [Params(100, 1000)]
    public int LookupCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"homura_bench_findby_{Guid.NewGuid()}.db");
        _connectionString = $"Data Source={_dbPath}";

        ConnectionManager.SetDefaultConnection(_instanceId, _connectionString, typeof(SQLiteConnection));
        var svManager = new DataVersionManager();
        svManager.CurrentConnection = ConnectionManager.DefaultConnection;
        var plan = new ChangePlan<VersionOrigin>(VersioningMode.ByTick);
        plan.AddVersionChangePlan(new BenchmarkChangePlan(VersioningMode.ByTick));
        svManager.RegisterChangePlan(plan);
        svManager.SetDefault();
        svManager.UpgradeToTargetVersion();

        // Seed 1000 rows and remember their Ids
        const int seedCount = 1000;
        _ids = new Guid[seedCount];
        var dao = new BenchmarkEntityDao(typeof(VersionOrigin));
        for (int i = 0; i < seedCount; i++)
        {
            _ids[i] = Guid.NewGuid();
            dao.InsertAsync(new BenchmarkEntity
            {
                Id = _ids[i],
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
    public async Task<int> Homura_FindById()
    {
        var dao = new BenchmarkEntityDao(typeof(VersionOrigin));
        await using var conn = await dao.GetConnectionAsync();
        int matched = 0;
        for (int i = 0; i < LookupCount; i++)
        {
            var id = _ids[i % _ids.Length];
            var dict = new Dictionary<string, object> { { "Id", id } };
            await foreach (var row in dao.FindByAsync(dict, conn))
            {
                matched++;
            }
        }
        return matched;
    }

    [Benchmark(Description = "Homura ORM (Generated)")]
    public async Task<int> Homura_Generated_FindById()
    {
        var dao = new GeneratedBenchmarkEntityDao(typeof(VersionOrigin));
        await using var conn = await dao.GetConnectionAsync();
        int matched = 0;
        for (int i = 0; i < LookupCount; i++)
        {
            var id = _ids[i % _ids.Length];
            var dict = new Dictionary<string, object> { { "Id", id } };
            await foreach (var row in dao.FindByAsync(dict, conn))
            {
                matched++;
            }
        }
        return matched;
    }

    [Benchmark(Description = "Homura ORM (Generated Typed PK)")]
    public async Task<int> Homura_Generated_FindByPrimaryKey()
    {
        var dao = new GeneratedBenchmarkEntityDao(typeof(VersionOrigin));
        await using var conn = await dao.GetConnectionAsync();
        int matched = 0;
        for (int i = 0; i < LookupCount; i++)
        {
            var id = _ids[i % _ids.Length];
            var row = await dao.FindByPrimaryKeyAsync(id, conn);
            if (row != null) matched++;
        }
        return matched;
    }

    [Benchmark(Description = "Dapper")]
    public async Task<int> Dapper_FindById()
    {
        using var conn = new SQLiteConnection(_connectionString);
        await conn.OpenAsync();
        int matched = 0;
        for (int i = 0; i < LookupCount; i++)
        {
            var id = _ids[i % _ids.Length];
            var results = await conn.QueryAsync<EfBenchmarkEntity>(
                "SELECT Id, Name, Value, Description, CreatedAt FROM BenchmarkEntity WHERE Id = @Id",
                new { Id = id });
            foreach (var _ in results) matched++;
        }
        return matched;
    }

    [Benchmark(Description = "EF Core")]
    public async Task<int> EfCore_FindById()
    {
        var options = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseSqlite($"Data Source={_dbPath};Pooling=False")
            .Options;
        using var context = new BenchmarkDbContext(options);
        int matched = 0;
        for (int i = 0; i < LookupCount; i++)
        {
            var id = _ids[i % _ids.Length].ToString();
            var row = await context.BenchmarkEntities.FirstOrDefaultAsync(e => e.Id == id);
            if (row != null) matched++;
        }
        return matched;
    }
}
