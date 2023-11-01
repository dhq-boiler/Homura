using Homura.ORM;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;
using System.Threading.Tasks;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class Roki_ChangePlan_Version_2 : ChangePlan<Roki, Version_2>
    {
        public Roki_ChangePlan_Version_2(VersioningMode mode) : base("Roki_2", PostMigrationVerification.TableExists, mode, MigrationAction.AlterTable)
        {
        }

        public override async Task CreateTable(IConnection connection)
        {
            var dao = new RokiDao(TargetVersion.GetType());
            dao.CurrentConnection = connection;
            await dao.CreateTableIfNotExistsAsync();
            ++ModifiedCount;
        }

        public override async Task DropTable(IConnection connection)
        {
            var dao = new RokiDao(TargetVersion.GetType());
            dao.CurrentConnection = connection;
            await dao.DropTableIfExistsAsync();
            ++ModifiedCount;
        }

        public override async Task UpgradeToTargetVersion(IConnection connection)
        {
            var dao = new RokiDao(TargetVersion.GetType());
            await dao.AdjustColumnsAsync(typeof(Version_1), TargetVersion.GetType());
        }
    }
}