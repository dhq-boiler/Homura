using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class Frey_ChangePlan_VersionOrigin : ChangePlan<Frey, VersionOrigin>
    {
        public Frey_ChangePlan_VersionOrigin(VersioningMode mode) : base("Frey", PostMigrationVerification.TableExists, mode)
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
            dao.DropTable();
            ++ModifiedCount;
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}