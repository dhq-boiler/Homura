
using Homura.ORM;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class BetaChangePlan_Version_2 : BetaChangePlan_Abstract<Version_2>
    {
        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
