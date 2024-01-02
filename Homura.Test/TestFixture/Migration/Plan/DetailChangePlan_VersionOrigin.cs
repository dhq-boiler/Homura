

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using System.Threading.Tasks;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class DetailChangePlan_VersionOrigin : DetailChangePlan_Abstract<VersionOrigin>
    {
        public DetailChangePlan_VersionOrigin(VersioningMode mode) : base("Detail", PostMigrationVerification.TableExists, mode)
        {
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
