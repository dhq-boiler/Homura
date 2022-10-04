

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;

namespace Sunctum.Domain.Data.Dao.Migration.Plan
{
    internal class Valkyrie_0_ChangePlan_VersionOrigin : ChangePlan<Valkyrie_0, VersionOrigin>
    {
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
            dao.DropTable();
            ++ModifiedCount;
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
