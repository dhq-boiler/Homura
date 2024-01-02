using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;
using System.Threading.Tasks;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class Frey_ChangePlan_Version_1 : ChangePlan<Frey, Version_1>
    {
        public Frey_ChangePlan_Version_1(VersioningMode mode) : base("Frey_1", PostMigrationVerification.TableExists, mode)
        {
        }

        public override void CreateTable(IConnection connection)
        {
            var dao = new FreyDao(TargetVersion.GetType());
            dao.CurrentConnection = connection;
            dao.CreateTableIfNotExists();
            ++ModifiedCount;
        }

        public override void DropTable(IConnection connection)
        {
            var dao = new FreyDao(TargetVersion.GetType());
            dao.CurrentConnection = connection;
            dao.DropTableIfExists();
            ++ModifiedCount;
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            var dao = new FreyDao(TargetVersion.GetType());
            dao.AdjustColumns(typeof(VersionOrigin), TargetVersion.GetType());
        }
    }
}