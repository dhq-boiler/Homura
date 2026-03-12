using Homura.ORM;
using Homura.ORM.Mapping;

namespace Homura.Benchmark.Entities;

[GenerateDao(DaoName = "GeneratedBenchmarkEntityDao")]
[DefaultVersion(typeof(VersionOrigin))]
public class BenchmarkEntity : EntityBaseObject
{
    [Column("Id", "NUMERIC", 0), PrimaryKey, Index]
    public Guid Id { get; set; }

    [Column("Name", "TEXT", 1)]
    public string Name { get; set; } = string.Empty;

    [Column("Value", "INTEGER", 2)]
    public int Value { get; set; }

    [Column("Description", "TEXT", 3)]
    public string Description { get; set; } = string.Empty;

    [Column("CreatedAt", "TEXT", 4)]
    public string CreatedAt { get; set; } = string.Empty;
}
