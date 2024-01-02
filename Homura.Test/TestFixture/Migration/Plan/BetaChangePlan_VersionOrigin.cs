

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using System.Threading.Tasks;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class BetaChangePlan_VersionOrigin : BetaChangePlan_Abstract<VersionOrigin>
    {
        public BetaChangePlan_VersionOrigin(VersioningMode mode) : base("Beta", PostMigrationVerification.TableExists, mode)
        {
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
