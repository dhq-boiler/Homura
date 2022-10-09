using Homura.ORM;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class Roki_ChangePlan_Version_2 : ChangePlan<Roki, Version_2>
    {
        public Roki_ChangePlan_Version_2(VersioningMode mode) : base("Roki_2", PostMigrationVerification.TableExists, mode, MigrationAction.AlterTable)
        {
        }

        public override void CreateTable(IConnection connection)
        {
            var dao = new RokiDao(TargetVersion.GetType());
            dao.CurrentConnection = connection;
            dao.CreateTableIfNotExists();
            ++ModifiedCount;
        }

        public override void DropTable(IConnection connection)
        {
            var dao = new RokiDao(TargetVersion.GetType());
            dao.CurrentConnection = connection;
            dao.DropTable();
            ++ModifiedCount;
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            var dao = new RokiDao(TargetVersion.GetType());
            dao.AdjustColumns(typeof(Version_1), TargetVersion.GetType());
        }
    }
}