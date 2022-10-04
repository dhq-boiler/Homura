

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;

namespace Sunctum.Domain.Data.Dao.Migration.Plan
{
    internal class PageChangePlan_VersionOrigin : ChangePlan<Page, VersionOrigin>
    {
        public override void CreateTable(IConnection connection)
        {
            PageDao dao = new PageDao(typeof(VersionOrigin));
            dao.CurrentConnection = connection;
            dao.CreateTableIfNotExists();
            ++ModifiedCount;
            dao.CreateIndexIfNotExists();
            ++ModifiedCount;
        }

        public override void DropTable(IConnection connection)
        {
            PageDao dao = new PageDao(typeof(VersionOrigin));
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
