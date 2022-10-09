
using Homura.ORM;
using Homura.ORM.Migration;
using Homura.ORM.Setup;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class BetaChangePlan_Version_2 : BetaChangePlan_Abstract<Version_2>
    {
        public BetaChangePlan_Version_2(VersioningMode mode) : base("Beta_2", PostMigrationVerification.TableExists, mode)
        {
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
