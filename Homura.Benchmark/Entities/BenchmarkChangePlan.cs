using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;

namespace Homura.Benchmark.Entities;

public class BenchmarkChangePlan : ChangePlan<BenchmarkEntity, VersionOrigin>
{
    public BenchmarkChangePlan(VersioningMode mode)
        : base("BenchmarkEntity", PostMigrationVerification.TableExists, mode) { }

    public override void CreateTable(IConnection connection)
    {
        var dao = new BenchmarkEntityDao(TargetVersion.GetType());
        dao.CurrentConnection = connection;
        dao.CreateTableIfNotExists();
        ++ModifiedCount;
    }

    public override void UpgradeToTargetVersion(IConnection connection)
    {
        CreateTable(connection);
    }
}
