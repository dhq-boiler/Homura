

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;
using System.Threading.Tasks;

namespace Sunctum.Domain.Data.Dao.Migration.Plan
{
    internal class PageChangePlan_VersionOrigin : ChangePlan<Page, VersionOrigin>
    {
        public PageChangePlan_VersionOrigin(VersioningMode mode) : base("Page", PostMigrationVerification.TableExists, mode)
        {
        }

        public override async Task CreateTable(IConnection connection)
        {
            PageDao dao = new PageDao(typeof(VersionOrigin));
            dao.CurrentConnection = connection;
            await dao.CreateTableIfNotExistsAsync();
            ++ModifiedCount;
            await dao.CreateIndexIfNotExistsAsync();
            ++ModifiedCount;
        }

        public override async Task DropTable(IConnection connection)
        {
            PageDao dao = new PageDao(typeof(VersionOrigin));
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
