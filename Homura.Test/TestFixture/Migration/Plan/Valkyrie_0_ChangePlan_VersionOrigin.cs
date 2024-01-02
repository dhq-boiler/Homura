

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

        public override void CreateTable(IConnection connection)
        {
            var dao = new Valkyrie_0_Dao(typeof(VersionOrigin));
            dao.CurrentConnection = connection;
            dao.CreateTableIfNotExists();
            ++ModifiedCount;
            dao.CreateIndexIfNotExists();
            ++ModifiedCount;
        }

        public override void DropTable(IConnection connection)
        {
            var dao = new Valkyrie_0_Dao(typeof(VersionOrigin));
            dao.CurrentConnection = connection;
            dao.DropTableIfExists();
            ++ModifiedCount;
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
