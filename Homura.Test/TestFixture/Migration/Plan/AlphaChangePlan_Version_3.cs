
using Homura.ORM;
using Homura.ORM.Migration;
using Homura.Test.TestFixture.Dao;
using System.Collections.Generic;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class AlphaChangePlan_Version_3 : AlphaChangePlan_Abstract<Version_3>
    {
        public override void UpgradeToTargetVersion(IConnection connection)
        {
            var dao = new AlphaDao(TargetVersion.GetType());
            dao.CurrentConnection = connection;

            dao.CreateTableIfNotExists();

            if (dao.CountAll() > 0)
            {
                dao.Delete(new Dictionary<string, object>());
            }

            dao.UpgradeTable(new VersionChangeUnit(typeof(Version_2), TargetVersion.GetType()));
        }
    }
}
