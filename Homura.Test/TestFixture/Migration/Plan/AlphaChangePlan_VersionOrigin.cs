

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using System.Threading.Tasks;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class AlphaChangePlan_VersionOrigin : AlphaChangePlan_Abstract<VersionOrigin>
    {
        public AlphaChangePlan_VersionOrigin(VersioningMode mode) : base("Alpha", PostMigrationVerification.TableExists, mode)
        {
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
