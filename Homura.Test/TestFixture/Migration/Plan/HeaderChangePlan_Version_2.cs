

using Homura.ORM;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class HeaderChangePlan_Version_2 : HeaderChangePlan_Abstract<Version_2>
    {
        public HeaderChangePlan_Version_2(VersioningMode mode) : base("Header_2", PostMigrationVerification.TableExists, mode)
        {
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            var dao = new HeaderDao(TargetVersion.GetType());
            dao.CurrentConnection = connection;

            dao.CreateTableIfNotExists();

            if (dao.CountAll() > 0)
            {
                dao.Delete(new Dictionary<string, object>());
            }

            dao.UpgradeTable(new VersionChangeUnit(typeof(Version_1), TargetVersion.GetType()), Mode);
        }
    }
}
