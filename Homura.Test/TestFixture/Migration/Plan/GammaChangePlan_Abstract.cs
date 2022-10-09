

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class GammaChangePlan_Abstract<V> : ChangePlan<Gamma, V> where V : VersionOrigin
    {
        public GammaChangePlan_Abstract(string targetTableName, PostMigrationVerification postMigrationVerification, VersioningMode mode) : base(targetTableName, postMigrationVerification, mode)
        {
        }

        public override void CreateTable(IConnection connection)
        {
            var dao = new GammaDao(TargetVersion.GetType());
            dao.CurrentConnection = connection;
            dao.CreateTableIfNotExists();
            ++ModifiedCount;
        }
    }
}
