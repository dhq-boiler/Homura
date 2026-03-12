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
public class UpdateBenchmark
{
    private string _dbPath = null!;
    private string _connectionString = null!;
    private readonly Guid _instanceId = Guid.NewGuid();
    private List<Guid> _ids = null!;

    [Params(100, 1000)]
    public int RecordCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"homura_bench_update_{Guid.NewGuid()}.db");
        _connectionString = $"Data Source={_dbPath}";
        _ids = new List<Guid>();

        // Setup via Homura migration (creates proper table structure)
        ConnectionManager.SetDefaultConnection(_instanceId, _connectionString, typeof(SQLiteConnection));
        var svManager = new DataVersionManager();
        svManager.CurrentConnection = ConnectionManager.DefaultConnection;
        var plan = new ChangePlan<VersionOrigin>(VersioningMode.ByTick);
        plan.AddVersionChangePlan(new BenchmarkChangePlan(VersioningMode.ByTick));
        svManager.RegisterChangePlan(plan);
        svManager.SetDefault();
        svManager.UpgradeToTargetVersion();

        // Seed data using Homura DAO
        var dao = new BenchmarkEntityDao(typeof(VersionOrigin));
        for (int i = 0; i < RecordCount; i++)
        {
            var id = Guid.NewGuid();
            _ids.Add(id);
            dao.InsertAsync(new BenchmarkEntity
            {
                Id = id,
                Name = $"Item_{i}",
                Value = i,
                Description = $"Description for item {i}",
                CreatedAt = DateTime.UtcNow.ToString("O")
            }).GetAwaiter().GetResult();
        }

        // Dispose Homura connections so other ORMs can access the DB
        ConnectionManager.DisposeDebris(_instanceId);
    }

    [IterationSetup]
    public void IterationSetup()
    {
        // Re-establish Homura connection for each iteration
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
    public async Task Homura_Update()
    {
        var dao = new BenchmarkEntityDao(typeof(VersionOrigin));
        await using var conn = await dao.GetConnectionAsync();
        foreach (var id in _ids)
        {
            await dao.UpdateAsync(new BenchmarkEntity
            {
                Id = id,
                Name = "Updated",
                Value = 999,
                Description = "Updated description",
                CreatedAt = DateTime.UtcNow.ToString("O")
            }, conn);
        }
    }

    [Benchmark(Description = "Homura ORM (Txn)")]
    public async Task Homura_UpdateWithTransaction()
    {
        var dao = new BenchmarkEntityDao(typeof(VersionOrigin));
        await using var conn = await dao.GetConnectionAsync();
        await using var txn = await conn.BeginTransactionAsync();
        foreach (var id in _ids)
        {
            await dao.UpdateAsync(new BenchmarkEntity
            {
                Id = id,
                Name = "Updated",
                Value = 999,
                Description = "Updated description",
                CreatedAt = DateTime.UtcNow.ToString("O")
            }, conn);
        }
        await txn.CommitAsync();
    }

    [Benchmark(Description = "Homura ORM (Bulk)")]
    public async Task Homura_UpdateBulk()
    {
        var dao = new BenchmarkEntityDao(typeof(VersionOrigin));
        var entities = _ids.Select(id => new BenchmarkEntity
        {
            Id = id,
            Name = "Updated",
            Value = 999,
            Description = "Updated description",
            CreatedAt = DateTime.UtcNow.ToString("O")
        });
        await dao.UpdateBulkAsync(entities);
    }

    [Benchmark(Description = "Homura Generated")]
    public async Task Homura_Update_Generated()
    {
        var dao = new GeneratedBenchmarkEntityDao(typeof(VersionOrigin));
        await using var conn = await dao.GetConnectionAsync();
        foreach (var id in _ids)
        {
            await dao.UpdateAsync(new BenchmarkEntity
            {
                Id = id,
                Name = "Updated",
                Value = 999,
                Description = "Updated description",
                CreatedAt = DateTime.UtcNow.ToString("O")
            }, conn);
        }
    }

    [Benchmark(Description = "Homura Generated (Txn)")]
    public async Task Homura_Update_GeneratedWithTransaction()
    {
        var dao = new GeneratedBenchmarkEntityDao(typeof(VersionOrigin));
        await using var conn = await dao.GetConnectionAsync();
        await using var txn = await conn.BeginTransactionAsync();
        foreach (var id in _ids)
        {
            await dao.UpdateAsync(new BenchmarkEntity
            {
                Id = id,
                Name = "Updated",
                Value = 999,
                Description = "Updated description",
                CreatedAt = DateTime.UtcNow.ToString("O")
            }, conn);
        }
        await txn.CommitAsync();
    }

    [Benchmark(Description = "Homura Generated (Bulk)")]
    public async Task Homura_Update_GeneratedBulk()
    {
        var dao = new GeneratedBenchmarkEntityDao(typeof(VersionOrigin));
        var entities = _ids.Select(id => new BenchmarkEntity
        {
            Id = id,
            Name = "Updated",
            Value = 999,
            Description = "Updated description",
            CreatedAt = DateTime.UtcNow.ToString("O")
        });
        await dao.UpdateBulkAsync(entities);
    }

    [Benchmark(Description = "Dapper")]
    public async Task Dapper_Update()
    {
        using var conn = new SQLiteConnection(_connectionString);
        await conn.OpenAsync();
        foreach (var id in _ids)
        {
            await conn.ExecuteAsync(
                "UPDATE BenchmarkEntity SET Name=@Name, Value=@Value, Description=@Description, CreatedAt=@CreatedAt WHERE Id=@Id",
                new
                {
                    Id = id.ToString(),
                    Name = "Updated",
                    Value = 999,
                    Description = "Updated description",
                    CreatedAt = DateTime.UtcNow.ToString("O")
                });
        }
    }

    [Benchmark(Description = "EF Core")]
    public async Task EfCore_Update()
    {
        var options = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseSqlite($"Data Source={_dbPath};Pooling=False")
            .Options;
        using var context = new BenchmarkDbContext(options);
        foreach (var id in _ids)
        {
            var now = DateTime.UtcNow.ToString("O");
            await context.Database.ExecuteSqlRawAsync(
                "UPDATE BenchmarkEntity SET Name=@p0, Value=@p1, Description=@p2, CreatedAt=@p3 WHERE Id=@p4",
                "Updated", 999, "Updated description", now, id.ToString());
        }
    }
}
