using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;
using System.Collections.Generic;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class Roki_ChangePlan_Version_1 : ChangePlan<Roki, Version_1>
    {
        public Roki_ChangePlan_Version_1(VersioningMode mode) : base("Roki_1", PostMigrationVerification.TableExists, mode)
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
            CreateTable(connection);

            var dao = new RokiDao(TargetVersion.GetType());
            if (dao.CountAll() > 0)
            {
                dao.Delete(new Dictionary<string, object>());
            }

            dao.UpgradeTable(new VersionChangeUnit(typeof(VersionOrigin), TargetVersion.GetType()), Mode);
        }
    }
}