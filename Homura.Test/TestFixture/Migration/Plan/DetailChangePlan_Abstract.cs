

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;
using System.Threading.Tasks;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal abstract class DetailChangePlan_Abstract<V> : ChangePlan<Detail, V> where V : VersionOrigin
    {
        protected DetailChangePlan_Abstract(string targetTableName, PostMigrationVerification postMigrationVerification, VersioningMode mode) : base(targetTableName, postMigrationVerification, mode)
        {
        }

        public override async Task CreateTable(IConnection connection)
        {
            var dao = new DetailDao(TargetVersion.GetType());
            dao.CurrentConnection = connection;
            await dao.CreateTableIfNotExistsAsync();
            ++ModifiedCount;
        }

        public override async Task DropTable(IConnection connection)
        {
            var dao = new DetailDao(TargetVersion.GetType());
            dao.CurrentConnection = connection;
            await dao.DropTableAsync();
            ++ModifiedCount;
        }
    }
}
