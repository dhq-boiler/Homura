

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class AlphaChangePlan_Abstract<V> : ChangePlan<Alpha, V> where V : VersionOrigin
    {
        public AlphaChangePlan_Abstract(VersioningMode mode) : base(mode)
        {
        }

        public override void CreateTable(IConnection connection)
        {
            var dao = new AlphaDao(TargetVersion.GetType());
            dao.CurrentConnection = connection;
            dao.CreateTableIfNotExists();
            ++ModifiedCount;
        }
    }
}
