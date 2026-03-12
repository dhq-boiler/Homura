using Homura.ORM;

namespace Homura.Benchmark.Entities;

public class BenchmarkEntityDao : Dao<BenchmarkEntity>
{
    public BenchmarkEntityDao() : base() { }
    public BenchmarkEntityDao(Type entityVersionType) : base(entityVersionType) { }
}
