using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;
using Homura.Test.TestFixture.Migration;
using System.Collections.Generic;

namespace Sunctum.Domain.Data.Dao.Migration.Plan
{
    internal class Valkyrie_1_ChangePlan_Version_1 : ChangePlan<Valkyrie_1, Version_1>
    {
        public Valkyrie_1_ChangePlan_Version_1(VersioningMode mode) : base(mode)
        {
        }

        public override void CreateTable(IConnection connection)
        {
            var dao = new Valkyrie_1_Dao(typeof(Version_1));
            dao.CurrentConnection = connection;
            dao.CreateTableIfNotExists();
            ++ModifiedCount;
            dao.CreateIndexIfNotExists();
            ++ModifiedCount;
        }

        public override void DropTable(IConnection connection)
        {
            var dao = new Valkyrie_1_Dao(typeof(Version_1));
            dao.CurrentConnection = connection;
            dao.DropTable();
            ++ModifiedCount;
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);

            var dao = new Valkyrie_1_Dao(TargetVersion.GetType());
            if (dao.CountAll() > 0)
            {
                dao.Delete(new Dictionary<string, object>());
            }

            dao.UpgradeTable(new VersionChangeUnit(typeof(VersionOrigin), TargetVersion.GetType()), Mode);
        }
    }
}
