using Microsoft.EntityFrameworkCore;

namespace Homura.Benchmark.Entities;

public class EfBenchmarkEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Description { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}

public class BenchmarkDbContext : DbContext
{
    private readonly string? _connectionString;

    public DbSet<EfBenchmarkEntity> BenchmarkEntities { get; set; } = null!;

    public BenchmarkDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public BenchmarkDbContext(DbContextOptions<BenchmarkDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured && _connectionString != null)
        {
            optionsBuilder.UseSqlite(_connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EfBenchmarkEntity>(entity =>
        {
            entity.ToTable("BenchmarkEntity");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.Name).HasColumnName("Name");
            entity.Property(e => e.Value).HasColumnName("Value");
            entity.Property(e => e.Description).HasColumnName("Description");
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
        });
    }
}
