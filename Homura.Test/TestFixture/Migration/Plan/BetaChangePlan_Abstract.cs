

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;
using System.Threading.Tasks;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class BetaChangePlan_Abstract<V> : ChangePlan<Beta, V> where V : VersionOrigin
    {
        public BetaChangePlan_Abstract(string targetTableName, PostMigrationVerification postMigrationVerification, VersioningMode mode) : base(targetTableName, postMigrationVerification, mode)
        {
        }

        public override async Task CreateTable(IConnection connection)
        {
            var dao = new BetaDao(TargetVersion.GetType());
            dao.CurrentConnection = connection;
            await dao.CreateTableIfNotExistsAsync();
            ++ModifiedCount;
        }
    }
}
