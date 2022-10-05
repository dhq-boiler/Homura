
using Homura.ORM;
using Homura.ORM.Setup;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class BetaChangePlan_Version_2 : BetaChangePlan_Abstract<Version_2>
    {
        public BetaChangePlan_Version_2(VersioningMode mode) : base(mode)
        {
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
