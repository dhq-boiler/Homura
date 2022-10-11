using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;
using System.Threading.Tasks;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class Frey_ChangePlan_VersionOrigin : ChangePlan<Frey, VersionOrigin>
    {
        public Frey_ChangePlan_VersionOrigin(VersioningMode mode) : base("Frey", PostMigrationVerification.TableExists, mode)
        {
        }

        public override async Task CreateTable(IConnection connection)
        {
            var dao = new FreyDao(TargetVersion.GetType());
            dao.CurrentConnection = connection;
            await dao.CreateTableIfNotExistsAsync();
            ++ModifiedCount;
        }

        public override async Task DropTable(IConnection connection)
        {
            var dao = new FreyDao(TargetVersion.GetType());
            dao.CurrentConnection = connection;
            await dao.DropTableAsync();
            ++ModifiedCount;
        }

        public override async Task UpgradeToTargetVersion(IConnection connection)
        {
            await CreateTable(connection);
        }
    }
}