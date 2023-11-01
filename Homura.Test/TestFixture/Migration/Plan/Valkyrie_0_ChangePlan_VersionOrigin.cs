

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;
using System.Threading.Tasks;

namespace Sunctum.Domain.Data.Dao.Migration.Plan
{
    internal class Valkyrie_0_ChangePlan_VersionOrigin : ChangePlan<Valkyrie_0, VersionOrigin>
    {
        public Valkyrie_0_ChangePlan_VersionOrigin(VersioningMode mode) : base("Valkyrie_0", PostMigrationVerification.TableExists, mode)
        {
        }

        public override async Task CreateTable(IConnection connection)
        {
            var dao = new Valkyrie_0_Dao(typeof(VersionOrigin));
            dao.CurrentConnection = connection;
            await dao.CreateTableIfNotExistsAsync();
            ++ModifiedCount;
            await dao.CreateIndexIfNotExistsAsync();
            ++ModifiedCount;
        }

        public override async Task DropTable(IConnection connection)
        {
            var dao = new Valkyrie_0_Dao(typeof(VersionOrigin));
            dao.CurrentConnection = connection;
            await dao.DropTableIfExistsAsync();
            ++ModifiedCount;
        }

        public override async Task UpgradeToTargetVersion(IConnection connection)
        {
            await CreateTable(connection);
        }
    }
}
